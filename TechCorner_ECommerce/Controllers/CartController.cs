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

        public IActionResult AddToCart(int variantId, int quantity = 1) {
            var cart = Cart;

            var item = cart.SingleOrDefault(i => i.ProductVariantId == variantId);

            if (item == null) {
                var variant = db.ProductVariants
                    .Include(v => v.Product)
                        .ThenInclude(p => p.Images)
                    .FirstOrDefault(v => v.Id == variantId);

                if (variant == null) {
                    return Redirect("/404");
                }

                var image = variant.Product.Images
                    .FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                    ?? variant.Product.Images.FirstOrDefault()?.ImageUrl
                    ?? "";

                item = new CartItemVM {
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product.Name,
                    Price = variant.Price,
                    Quantity = quantity,
                    ImageUrl = image
                };

                cart.Add(item);
            }
            else {
                item.Quantity += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, cart);

            int totalQty = cart.Sum(p => p.Quantity);

            return Json(new {
                success = true,
                quantity = totalQty
            });
        }

        public IActionResult RemoveCart(int variantId) {
            var cart = Cart;

            var item = cart.SingleOrDefault(p => p.ProductVariantId == variantId);

            if (item != null) {
                cart.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }

            int totalQty = cart.Sum(p => p.Quantity);
            decimal subTotal = cart.Sum(p => p.SubTotal);

            return Json(new {
                success = true,
                quantity = totalQty,
                subTotal
            });
        }

        public IActionResult UpdateQuantity(int variantId, int quantity) {
            var cart = Cart;

            var item = cart.SingleOrDefault(p => p.ProductVariantId == variantId);

            if (item != null) {
                item.Quantity = quantity;
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }

            int totalQty = cart.Sum(x => x.Quantity);
            decimal totalPrice = cart.Sum(x => x.SubTotal);

            return Json(new {
                success = true,
                quantity = totalQty,
                totalPrice
            });
        }
    }
}