using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;
using TechCorner_ECommerce.Helpers;

namespace TechCorner_ECommerce.Controllers {
    public class CartController : Controller {
        private readonly AppDbContext db;

        public CartController(AppDbContext context) {
            db = context;
        }

        public List<CartItemVM> Cart =>
            HttpContext.Session.Get<List<CartItemVM>>(MySetting.CART_KEY)
            ?? new List<CartItemVM>();

        public IActionResult Index() {
            return View(Cart);
        }

        private object CartSummary(List<CartItemVM> cart, bool success = true, string? message = null, int? available = null, bool removed = false) {
            return new {
                success,
                message,
                available,
                removed,
                quantity = cart.Sum(p => p.Quantity),
                subTotal = cart.Sum(p => p.SubTotal),
                totalPrice = cart.Sum(p => p.SubTotal)
            };
        }

        private CartItemVM CreateCartItem(Models.Product product, int quantity) {
            var image = product.ParentProduct.Images
                .FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? product.ParentProduct.Images.FirstOrDefault()?.ImageUrl
                ?? "";

            var attributes = product.ProductAttributeValues
                .Select(a => new AttributeVM {
                    Name = a.AttributeValue.ProductAttribute.Name,
                    Value = a.AttributeValue.Value
                }).ToList();

            return new CartItemVM {
                ProductId = product.Id,
                ProductName = product.ParentProduct.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = image,
                Attributes = attributes
            };
        }

        private void RefreshCartItem(CartItemVM item, Models.Product product) {
            var freshItem = CreateCartItem(product, item.Quantity);

            item.ProductName = freshItem.ProductName;
            item.Price = freshItem.Price;
            item.ImageUrl = freshItem.ImageUrl;
            item.Attributes = freshItem.Attributes;
        }

        private Models.Product? FindAvailableProduct(int productId) {
            return db.Products
                .Include(p => p.ParentProduct)
                    .ThenInclude(pp => pp.Images)
                .Include(p => p.ProductAttributeValues)
                    .ThenInclude(pav => pav.AttributeValue)
                        .ThenInclude(av => av.ProductAttribute)
                .FirstOrDefault(p =>
                    p.Id == productId &&
                    !p.IsDeleted &&
                    !p.ParentProduct.IsDeleted);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int quantity = 1) {
            var cart = Cart;

            if (productId <= 0 || quantity <= 0) {
                return Json(CartSummary(cart, false, "Invalid cart quantity."));
            }

            var product = FindAvailableProduct(productId);

            if (product == null) {
                return Json(CartSummary(cart, false, "Product is no longer available."));
            }

            if (product.StockQuantity <= 0) {
                return Json(CartSummary(cart, false, $"{product.ParentProduct.Name} is out of stock.", 0));
            }

            var matchingItems = cart.Where(i => i.ProductId == productId).ToList();
            var item = matchingItems.FirstOrDefault();
            var currentQuantity = matchingItems.Sum(i => i.Quantity);
            var newQuantity = currentQuantity + quantity;

            if (newQuantity > product.StockQuantity) {
                return Json(CartSummary(
                    cart,
                    false,
                    $"{product.ParentProduct.Name} only has {product.StockQuantity} item(s) in stock.",
                    product.StockQuantity));
            }

            if (item == null) {
                cart.Add(CreateCartItem(product, quantity));
            }
            else {
                foreach (var duplicate in matchingItems.Skip(1)) {
                    cart.Remove(duplicate);
                }

                item.Quantity = newQuantity;
                RefreshCartItem(item, product);
            }

            HttpContext.Session.Set(MySetting.CART_KEY, cart);

            return Json(CartSummary(cart));
        }



        [HttpGet]
        public IActionResult GetCartItems() {
            return PartialView("_CartContent", Cart);
        }

        [HttpGet]
        public IActionResult GetCartSummary() {
            var cart = Cart;

            return Json(new {
                success = true,
                quantity = cart.Sum(x => x.Quantity),
                subtotal = cart.Sum(x => x.SubTotal)
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveCart(int productId) {
            var cart = Cart;

            if (productId <= 0) {
                return Json(CartSummary(cart, false, "Invalid product."));
            }

            var items = cart.Where(p => p.ProductId == productId).ToList();

            if (items.Any()) {
                foreach (var item in items) {
                    cart.Remove(item);
                }

                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }

            return Json(CartSummary(cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity) {
            var cart = Cart;

            if (productId <= 0 || quantity <= 0) {
                return Json(CartSummary(cart, false, "Invalid cart quantity."));
            }

            var matchingItems = cart.Where(p => p.ProductId == productId).ToList();
            var item = matchingItems.FirstOrDefault();

            if (item == null) {
                return Json(CartSummary(cart, false, "Item was not found in your cart."));
            }

            foreach (var duplicate in matchingItems.Skip(1)) {
                cart.Remove(duplicate);
            }

            var product = FindAvailableProduct(productId);

            if (product == null) {
                cart.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
                return Json(CartSummary(cart, false, "Product is no longer available.", 0, true));
            }

            if (quantity > product.StockQuantity) {
                return Json(CartSummary(
                    cart,
                    false,
                    $"{product.ParentProduct.Name} only has {product.StockQuantity} item(s) in stock.",
                    product.StockQuantity));
            }

            item.Quantity = quantity;
            RefreshCartItem(item, product);
            HttpContext.Session.Set(MySetting.CART_KEY, cart);

            return Json(CartSummary(cart));
        }
    }
}
