using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;
using TechCorner_ECommerce.Helpers;

namespace TechCorner_ECommerce.Controllers {
    public class CartController : Controller {
        private readonly AppDbContext db;

        public CartController(AppDbContext context) {
           db = context;
        }
        
        public List<CartItemVM> Cart => HttpContext.Session.Get<List<CartItemVM>>(MySetting.CART_KEY) ?? new List<CartItemVM>();

        public IActionResult Index() {
            return View(Cart);
        }
        public IActionResult AddToCart(int id, int quantity = 1) {
            var cart = Cart;
            var item = cart.SingleOrDefault(i => i.ProductId == id);
            if (item == null) { 
                var product = db.Products.SingleOrDefault(p => p.Id == id);
                if (product == null) {
                    return Redirect("/404");
                }
                item  = new CartItemVM {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                };
                cart.Add(item);
            }
            else {
                item.Quantity += quantity;
            }
            HttpContext.Session.Set(MySetting.CART_KEY, cart);

            // Quantity của minicart
            int totalQty = cart.Sum(p => p.Quantity);

            return Json(new { 
                success = true,
                quantity = totalQty
            });
        }

        public IActionResult RemoveCart(int id) {
            var cart = Cart;
            var item = cart.SingleOrDefault(p => p.ProductId == id);
            if (item != null) {
                cart.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }
            int totalQty = cart.Sum(p => p.Quantity);

            return Json(new {
                success = true,
                quantity = totalQty
            });
        }

        public IActionResult UpdateQuantity(int id, int quantity) {
            var cart = Cart;
            var item = cart.SingleOrDefault(p => p.ProductId == id);

            if (item != null) {
                item.Quantity = quantity;
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }

            int totalQty = cart.Sum(x => x.Quantity);
            double totalPrice = cart.Sum(x => x.TotalPrice);

            return Json(new {
                success = true,
                quantity = totalQty,
                totalPrice
            });
        }
    }
}
