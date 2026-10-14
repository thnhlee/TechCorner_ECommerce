using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.ViewModels;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.Services;

namespace TechCorner_ECommerce.Controllers {
    public class CartController : Controller {
        private readonly ICartService cartService;

        public CartController(ICartService cartService) {
            this.cartService = cartService;
        }

        public List<CartItemVM> Cart =>
            HttpContext.Session.Get<List<CartItemVM>>(MySetting.CART_KEY)
            ?? new List<CartItemVM>();

        public IActionResult Index() {
            return View(Cart);
        }

        ///////////////////////// CART JSON SUMMARY /////////////////////////
        private static object CartSummary(CartOperationResult result) {
            return new {
                success = result.Success,
                message = result.Message,
                available = result.Available,
                removed = result.Removed,
                quantity = result.Quantity,
                subTotal = result.SubTotal,
                totalPrice = result.SubTotal
            };
        }

        ///////////////////////// ADD TO CART /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int quantity = 1) {
            var cart = Cart;
            var result = cartService.AddToCart(cart, productId, quantity);

            if (result.Success) {
                HttpContext.Session.Set(MySetting.CART_KEY, result.Cart);
            }

            return Json(CartSummary(result));
        }



        ///////////////////////// CART PARTIAL /////////////////////////
        [HttpGet]
        public IActionResult GetCartItems() {
            return PartialView("_CartContent", Cart);
        }

        ///////////////////////// CART SUMMARY /////////////////////////
        [HttpGet]
        public IActionResult GetCartSummary() {
            var cart = Cart;
            var subTotal = cartService.GetSubTotal(cart);

            return Json(new {
                success = true,
                quantity = cartService.GetQuantity(cart),
                subtotal = subTotal
            });
        }


        ///////////////////////// REMOVE CART ITEM /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveCart(int productId) {
            var cart = Cart;
            var result = cartService.RemoveFromCart(cart, productId);

            if (result.Success) {
                HttpContext.Session.Set(MySetting.CART_KEY, result.Cart);
            }

            return Json(CartSummary(result));
        }

        ///////////////////////// UPDATE QUANTITY /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity) {
            var cart = Cart;
            var result = cartService.UpdateQuantity(cart, productId, quantity);

            HttpContext.Session.Set(MySetting.CART_KEY, result.Cart);

            return Json(CartSummary(result));
        }
    }
}
