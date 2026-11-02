using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Controllers {
    [Authorize]
    public class OrderController : Controller {
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService) {
            this.orderService = orderService;
        }

        private List<CartItemVM> Cart =>
            HttpContext.Session.Get<List<CartItemVM>>(MySetting.CART_KEY) ?? new List<CartItemVM>();

        ///////////////////////// CHECKOUT /////////////////////////
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Checkout() {
            var result = await orderService.GetCheckoutAsync(User, Cart);

            if (result.CartEmpty) {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            return View(result.Model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVM model) {
            orderService.PrepareCheckoutSubmission(model, Cart);

            if (!model.Items.Any()) {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            AddModelErrors(orderService.ValidateCheckout(model));

            if (!ModelState.IsValid) {
                return View(model);
            }

            var result = await orderService.PlaceOrderAsync(User, model);

            if (result.CartEmpty) {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (!result.Succeeded) {
                AddModelErrors(result.Errors);
                return View(result.Model);
            }

            HttpContext.Session.Remove(MySetting.CART_KEY);

            return RedirectToAction("Success", new { id = result.OrderCode });
        }

        ///////////////////////// ORDER LIST /////////////////////////
        [HttpGet]
        public async Task<IActionResult> Index() {
            var result = await orderService.GetUserOrdersAsync(User);

            if (result.RequiresLogin) {
                return RedirectToAction("Login", "Account");
            }

            return View(result.Value);
        }

        ///////////////////////// ORDER DETAIL /////////////////////////
        [HttpGet]
        public async Task<IActionResult> Detail(string id) {
            var result = await orderService.GetUserOrderDetailAsync(User, id);

            if (result.RequiresLogin) {
                return RedirectToAction("Login", "Account");
            }

            if (result.Value == null) {
                return NotFound();
            }

            return View(result.Value);
        }

        ///////////////////////// CHECKOUT SUCCESS /////////////////////////
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Success(string id) {
            var order = await orderService.GetSuccessOrderAsync(User, id);

            if (order == null) {
                return NotFound();
            }

            return View(order);
        }

        private void AddModelErrors(IEnumerable<OrderValidationError> errors) {
            foreach (var error in errors) {
                ModelState.AddModelError(error.Key, error.Message);
            }
        }
    }
}
