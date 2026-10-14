using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.Models.Enums;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public class OrderService : IOrderService {
        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUniqueCodeService uniqueCodeService;
        private readonly IBackgroundTaskQueue backgroundTaskQueue;
        private readonly ILogger<OrderService> logger;
        private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase) {
            "COD"
        };

        public OrderService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IUniqueCodeService uniqueCodeService,
            IBackgroundTaskQueue backgroundTaskQueue,
            ILogger<OrderService> logger) {
            db = context;
            this.userManager = userManager;
            this.uniqueCodeService = uniqueCodeService;
            this.backgroundTaskQueue = backgroundTaskQueue;
            this.logger = logger;
        }

        ///////////////////////// CHECKOUT PREFILL /////////////////////////
        public async Task<CheckoutPageResult> GetCheckoutAsync(ClaimsPrincipal principal, List<CartItemVM> cart) {
            if (!cart.Any()) {
                return new CheckoutPageResult {
                    CartEmpty = true
                };
            }

            var user = await userManager.GetUserAsync(principal);
            var address = user == null
                ? null
                : await db.Addresses.FirstOrDefaultAsync(x => x.UserId == user.Id);

            // Account address is only an autofill source. Order keeps its own receiver snapshot.
            return new CheckoutPageResult {
                Model = new CheckoutVM {
                    ReceiverName = address?.ReceiverName ?? user?.FullName,
                    Email = user?.Email,
                    Phone = address?.Phone ?? user?.PhoneNumber,
                    FullAddress = address?.FullAddress,
                    Items = cart
                }
            };
        }

        ///////////////////////// CHECKOUT SUBMIT /////////////////////////
        public CheckoutVM PrepareCheckoutSubmission(CheckoutVM model, List<CartItemVM> cart) {
            NormalizeCheckoutModel(model);

            model.Items = cart
                .GroupBy(x => x.ProductId)
                .Select(group => {
                    var item = group.First();
                    item.Quantity = group.Sum(x => x.Quantity);
                    return item;
                })
                .ToList();

            return model;
        }

        ///////////////////////// CHECKOUT VALIDATION /////////////////////////
        public IReadOnlyList<OrderValidationError> ValidateCheckout(CheckoutVM model) {
            var errors = new List<OrderValidationError>();

            if (string.IsNullOrWhiteSpace(model.ReceiverName)) {
                errors.Add(new OrderValidationError {
                    Key = nameof(model.ReceiverName),
                    Message = "Receiver name is required"
                });
            }

            if (string.IsNullOrWhiteSpace(model.Email)) {
                errors.Add(new OrderValidationError {
                    Key = nameof(model.Email),
                    Message = "Email is required"
                });
            }

            if (string.IsNullOrWhiteSpace(model.Phone)) {
                errors.Add(new OrderValidationError {
                    Key = nameof(model.Phone),
                    Message = "Phone is required"
                });
            }

            if (string.IsNullOrWhiteSpace(model.FullAddress)) {
                errors.Add(new OrderValidationError {
                    Key = nameof(model.FullAddress),
                    Message = "Address is required"
                });
            }

            if (!AllowedPaymentMethods.Contains(model.PaymentMethod)) {
                errors.Add(new OrderValidationError {
                    Key = nameof(model.PaymentMethod),
                    Message = "Invalid payment method."
                });
            }

            return errors;
        }

        ///////////////////////// PLACE ORDER /////////////////////////
        public async Task<PlaceOrderResult> PlaceOrderAsync(ClaimsPrincipal principal, CheckoutVM model) {
            var result = new PlaceOrderResult {
                Model = model
            };

            if (!model.Items.Any()) {
                result.CartEmpty = true;
                return result;
            }

            result.Errors.AddRange(ValidateCheckout(model));

            if (result.Errors.Any()) {
                return result;
            }

            var user = await userManager.GetUserAsync(principal);
            var productIds = model.Items.Select(x => x.ProductId).ToList();
            var products = await db.Products
                .Include(x => x.ParentProduct)
                .Where(x => productIds.Contains(x.Id))
                .ToListAsync();

            // Validate against current database state, not only the session cart.
            foreach (var cartItem in model.Items) {
                var product = products.FirstOrDefault(x => x.Id == cartItem.ProductId);

                if (product == null || product.IsDeleted || product.ParentProduct.IsDeleted) {
                    result.Errors.Add(new OrderValidationError {
                        Message = $"{cartItem.ProductName} is no longer available."
                    });
                    return result;
                }

                if (cartItem.Quantity <= 0) {
                    result.Errors.Add(new OrderValidationError {
                        Message = "Invalid cart quantity."
                    });
                    return result;
                }

                if (product.StockQuantity < cartItem.Quantity) {
                    result.Errors.Add(new OrderValidationError {
                        Message = $"{product.ParentProduct.Name} only has {product.StockQuantity} item(s) in stock."
                    });
                    return result;
                }
            }

            var productLookup = products.ToDictionary(x => x.Id);
            var orderTotal = model.Items.Sum(cartItem =>
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

                var order = new Order {
                    OrderCode = uniqueCodeService.CreateOrderCode(),
                    UserId = user?.Id,
                    ReceiverName = model.ReceiverName ?? string.Empty,
                    ReceiverEmail = model.Email ?? user?.Email ?? string.Empty,
                    ReceiverPhone = model.Phone ?? string.Empty,
                    ShippingAddress = model.FullAddress ?? string.Empty,
                    OrderDate = DateTime.Now,
                    TotalPrice = orderTotal,
                    Status = OrderStatus.Pending,
                    // Address remains for autofill/account linkage; receiver fields above are the order snapshot.
                    Address = address,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.Orders.Add(order);

                foreach (var cartItem in model.Items) {
                    var product = products.First(x => x.Id == cartItem.ProductId);

                    if (product.StockQuantity < cartItem.Quantity) {
                        result.Errors.Add(new OrderValidationError {
                            Message = $"{product.ParentProduct.Name} only has {product.StockQuantity} item(s) in stock."
                        });
                        await transaction.RollbackAsync();
                        return result;
                    }

                    db.OrderDetails.Add(new OrderDetail {
                        Order = order,
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        Price = product.Price
                    });

                    // Stock is reduced in the same transaction as the order.
                    product.StockQuantity -= cartItem.Quantity;
                    product.UpdatedAt = DateTime.Now;
                }

                var paymentStatus = model.PaymentMethod == "COD"
                    ? PaymentStatus.Pending
                    : PaymentStatus.Pending;

                db.Payments.Add(new Payment {
                    Order = order,
                    PaymentMethod = model.PaymentMethod,
                    Status = paymentStatus,
                    PaidAt = paymentStatus == PaymentStatus.Paid ? DateTime.Now : null,
                    CreatedAt = DateTime.Now
                });

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                QueueOrderConfirmationEmail(order, model.Items, productLookup);

                result.Succeeded = true;
                result.OrderCode = order.OrderCode;
                return result;
            }
            catch {
                await transaction.RollbackAsync();
                result.Errors.Add(new OrderValidationError {
                    Message = "Could not place order. Please try again."
                });
                return result;
            }
        }

        ///////////////////////// USER ORDERS /////////////////////////
        public async Task<UserOrderResult<List<OrderListVM>>> GetUserOrdersAsync(ClaimsPrincipal principal) {
            var user = await userManager.GetUserAsync(principal);

            if (user == null) {
                return new UserOrderResult<List<OrderListVM>> {
                    RequiresLogin = true
                };
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

            return new UserOrderResult<List<OrderListVM>> {
                Value = orders
            };
        }

        public async Task<UserOrderResult<Order>> GetUserOrderDetailAsync(ClaimsPrincipal principal, string orderCode) {
            var user = await userManager.GetUserAsync(principal);

            if (user == null) {
                return new UserOrderResult<Order> {
                    RequiresLogin = true
                };
            }

            var order = await db.Orders
                .Include(x => x.Payments)
                .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Product!)
                    .ThenInclude(x => x.ParentProduct)
                .FirstOrDefaultAsync(x => x.OrderCode == orderCode && x.UserId == user.Id);

            return new UserOrderResult<Order> {
                Value = order
            };
        }

        public async Task<Order?> GetSuccessOrderAsync(ClaimsPrincipal principal, string orderCode) {
            var user = await userManager.GetUserAsync(principal);

            var query = db.Orders
                .Include(x => x.Payments)
                .AsQueryable();

            query = user == null
                ? query.Where(x => x.OrderCode == orderCode && x.UserId == null)
                : query.Where(x => x.OrderCode == orderCode && (x.UserId == user.Id || x.UserId == null));

            return await query.FirstOrDefaultAsync();
        }

        private static void NormalizeCheckoutModel(CheckoutVM model) {
            model.ReceiverName = model.ReceiverName?.Trim();
            model.Email = model.Email?.Trim();
            model.Phone = model.Phone?.Trim();
            model.FullAddress = model.FullAddress?.Trim();
            model.PaymentMethod = model.PaymentMethod?.Trim().ToUpperInvariant() ?? "";
        }

        private void QueueOrderConfirmationEmail(
            Order order,
            IEnumerable<CartItemVM> cart,
            IReadOnlyDictionary<int, Product> products) {
            if (string.IsNullOrWhiteSpace(order.ReceiverEmail)) {
                return;
            }

            try {
                var receiverEmail = order.ReceiverEmail;
                var subject = $"TechCorner order {order.OrderCode} confirmed";
                var orderCode = order.OrderCode;
                var model = new OrderConfirmationEmailVM {
                    ReceiverName = order.ReceiverName,
                    OrderCode = order.OrderCode,
                    ShippingAddress = order.ShippingAddress,
                    ReceiverPhone = order.ReceiverPhone,
                    TotalPrice = order.TotalPrice,
                    Items = cart.Select(item => {
                        var product = products[item.ProductId];

                        return new OrderConfirmationEmailItemVM {
                            ProductName = product.ParentProduct.Name,
                            Quantity = item.Quantity,
                            Price = product.Price
                        };
                    }).ToList()
                };

                backgroundTaskQueue.QueueBackgroundWorkItem(async (serviceProvider, cancellationToken) => {
                    try {
                        var renderer = serviceProvider.GetRequiredService<IEmailTemplateRenderer>();
                        var emailService = serviceProvider.GetRequiredService<IEmailService>();
                        var body = await renderer.RenderAsync("/Views/Emails/OrderConfirmation.cshtml", model);

                        cancellationToken.ThrowIfCancellationRequested();
                        await emailService.SendAsync(receiverEmail, subject, body);
                    }
                    catch (Exception ex) {
                        var backgroundLogger = serviceProvider.GetRequiredService<ILogger<OrderService>>();
                        backgroundLogger.LogError(ex, "Could not send order confirmation email for order {OrderCode}.", orderCode);
                    }
                });
            }
            catch (Exception ex) {
                logger.LogError(ex, "Could not queue order confirmation email for order {OrderCode}.", order.OrderCode);
            }
        }
    }
}
