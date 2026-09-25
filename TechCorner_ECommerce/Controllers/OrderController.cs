using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.Models.Enums;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Controllers {
    [Authorize]
    public class OrderController : Controller {
        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUniqueCodeService _uniqueCodeService;
        private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase) {
            "COD"
        };

        public OrderController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IUniqueCodeService uniqueCodeService) {
            db = context;
            _userManager = userManager;
            _uniqueCodeService = uniqueCodeService;
        }

        private List<CartItemVM> Cart =>
            HttpContext.Session.Get<List<CartItemVM>>(MySetting.CART_KEY) ?? new List<CartItemVM>();

        private static void NormalizeCheckoutModel(CheckoutVM model) {
            model.ReceiverName = model.ReceiverName?.Trim();
            model.Email = model.Email?.Trim();
            model.Phone = model.Phone?.Trim();
            model.FullAddress = model.FullAddress?.Trim();
            model.PaymentMethod = model.PaymentMethod?.Trim().ToUpperInvariant() ?? "";
        }

        private void ValidateCheckoutModel(CheckoutVM model) {
            if (string.IsNullOrWhiteSpace(model.ReceiverName)) {
                ModelState.AddModelError(nameof(model.ReceiverName), "Receiver name is required");
            }

            if (string.IsNullOrWhiteSpace(model.Email)) {
                ModelState.AddModelError(nameof(model.Email), "Email is required");
            }

            if (string.IsNullOrWhiteSpace(model.Phone)) {
                ModelState.AddModelError(nameof(model.Phone), "Phone is required");
            }

            if (string.IsNullOrWhiteSpace(model.FullAddress)) {
                ModelState.AddModelError(nameof(model.FullAddress), "Address is required");
            }

            if (!AllowedPaymentMethods.Contains(model.PaymentMethod)) {
                ModelState.AddModelError(nameof(model.PaymentMethod), "Invalid payment method.");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Checkout() {
            var cart = Cart;

            if (!cart.Any()) {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var address = user == null
                ? null
                : await db.Addresses.FirstOrDefaultAsync(x => x.UserId == user.Id);

            var model = new CheckoutVM {
                ReceiverName = address?.ReceiverName ?? user?.FullName,
                Email = user?.Email,
                Phone = address?.Phone ?? user?.PhoneNumber,
                FullAddress = address?.FullAddress,
                Items = cart
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVM model) {
            NormalizeCheckoutModel(model);

            var cart = Cart
                .GroupBy(x => x.ProductId)
                .Select(group => {
                    var item = group.First();
                    item.Quantity = group.Sum(x => x.Quantity);
                    return item;
                })
                .ToList();

            model.Items = cart;

            if (!cart.Any()) {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            ValidateCheckoutModel(model);

            if (!ModelState.IsValid) {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            var productIds = cart.Select(x => x.ProductId).ToList();
            var products = await db.Products
                .Include(x => x.ParentProduct)
                .Where(x => productIds.Contains(x.Id))
                .ToListAsync();

            foreach (var cartItem in cart) {
                var product = products.FirstOrDefault(x => x.Id == cartItem.ProductId);

                if (product == null || product.IsDeleted || product.ParentProduct.IsDeleted) {
                    ModelState.AddModelError("", $"{cartItem.ProductName} is no longer available.");
                    return View(model);
                }

                if (cartItem.Quantity <= 0) {
                    ModelState.AddModelError("", "Invalid cart quantity.");
                    return View(model);
                }

                if (product.StockQuantity < cartItem.Quantity) {
                    ModelState.AddModelError("", $"{product.ParentProduct.Name} only has {product.StockQuantity} item(s) in stock.");
                    return View(model);
                }
            }

            var productLookup = products.ToDictionary(x => x.Id);
            var orderTotal = cart.Sum(cartItem =>
                productLookup[cartItem.ProductId].Price * cartItem.Quantity);

            await using var transaction = await db.Database.BeginTransactionAsync();

            try {
                var address = user == null
                    ? null
                    : await db.Addresses.FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (user != null && address == null) {
                    address = new Address {
                        UserId = user.Id,
                        ReceiverName = model.ReceiverName?.Trim(),
                        Phone = model.Phone?.Trim(),
                        FullAddress = model.FullAddress?.Trim(),
                        CreatedAt = DateTime.Now
                    };

                    db.Addresses.Add(address);
                }
                else if (address != null) {
                    address.ReceiverName = model.ReceiverName?.Trim();
                    address.Phone = model.Phone?.Trim();
                    address.FullAddress = model.FullAddress?.Trim();
                    address.UpdatedAt = DateTime.Now;
                }

                await db.SaveChangesAsync();

                var order = new Order {
                    OrderCode = _uniqueCodeService.CreateOrderCode(),
                    UserId = user?.Id,
                    ReceiverName = model.ReceiverName ?? string.Empty,
                    ReceiverEmail = model.Email ?? user?.Email ?? string.Empty,
                    ReceiverPhone = model.Phone ?? string.Empty,
                    ShippingAddress = model.FullAddress ?? string.Empty,
                    OrderDate = DateTime.Now,
                    TotalPrice = orderTotal,
                    Status = OrderStatus.Pending,
                    AddressId = address?.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.Orders.Add(order);
                await db.SaveChangesAsync();

                foreach (var cartItem in cart) {
                    var product = products.First(x => x.Id == cartItem.ProductId);

                    if (product.StockQuantity < cartItem.Quantity) {
                        ModelState.AddModelError("", $"{product.ParentProduct.Name} only has {product.StockQuantity} item(s) in stock.");
                        await transaction.RollbackAsync();
                        return View(model);
                    }

                    db.OrderDetails.Add(new OrderDetail {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        Price = product.Price
                    });

                    product.StockQuantity -= cartItem.Quantity;
                    product.UpdatedAt = DateTime.Now;
                }

                var paymentStatus = model.PaymentMethod == "COD"
                    ? PaymentStatus.Pending
                    : PaymentStatus.Pending;

                db.Payments.Add(new Payment {
                    OrderId = order.Id,
                    PaymentMethod = model.PaymentMethod,
                    Status = paymentStatus,
                    PaidAt = paymentStatus == PaymentStatus.Paid ? DateTime.Now : null,
                    CreatedAt = DateTime.Now
                });

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove(MySetting.CART_KEY);

                return RedirectToAction("Success", new { id = order.OrderCode });
            }
            catch {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Could not place order. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            var orders = await db.Orders
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new OrderListVM {
                    Id = x.Id,
                    OrderCode = x.OrderCode,
                    OrderDate = x.OrderDate,
                    TotalPrice = x.TotalPrice,
                    Status = x.Status.ToString(),
                    PaymentStatus = x.Payments
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => p.Status.ToString())
                        .FirstOrDefault() ?? PaymentStatus.Pending.ToString()
                })
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id) {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            var order = await db.Orders
                .Include(x => x.Payments)
                .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Product!)
                    .ThenInclude(x => x.ParentProduct)
                .FirstOrDefaultAsync(x => x.OrderCode == id && x.UserId == user.Id);

            if (order == null) {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Success(string id) {
            var user = await _userManager.GetUserAsync(User);

            var query = db.Orders
                .Include(x => x.Payments)
                .AsQueryable();

            query = user == null
                ? query.Where(x => x.OrderCode == id && x.UserId == null)
                : query.Where(x => x.OrderCode == id && (x.UserId == user.Id || x.UserId == null));

            var order = await query.FirstOrDefaultAsync();

            if (order == null) {
                return NotFound();
            }

            return View(order);
        }
    }
}
