using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.ViewComponents {
    public class CartViewComponent : ViewComponent {
        public IViewComponentResult Invoke() {
            var cart = HttpContext.Session.Get<List<CartItemVM>>(MySetting.CART_KEY) ?? new List<CartItemVM>();
            return View("CartPanel", new CartItemVM { 
                Quantity = cart.Sum(c => c.Quantity), 
            });
        }
    }
}
