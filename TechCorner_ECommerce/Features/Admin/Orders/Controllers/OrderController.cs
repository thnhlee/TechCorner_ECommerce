using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Models.Enums;
using TechCorner_ECommerce.Services;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class OrderController : Controller {
        private readonly IAdminOrderQueryService adminOrderQueryService;
        private readonly IOrderStatusService orderStatusService;

        public OrderController(
            IAdminOrderQueryService adminOrderQueryService,
            IOrderStatusService orderStatusService) {
            this.adminOrderQueryService = adminOrderQueryService;
            this.orderStatusService = orderStatusService;
        }

        ///////////////////////// LIST /////////////////////////
        public IActionResult Index(string? keyword, OrderStatus? status, PaymentStatus? paymentStatus, int? page) {
            const int pageSize = 8;
            var pageNumber = page ?? 1;

            ViewBag.SearchQuery = keyword;
            ViewBag.Status = status;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.OrderStatuses = Enum.GetValues<OrderStatus>();
            ViewBag.PaymentStatuses = Enum.GetValues<PaymentStatus>();

            var result = adminOrderQueryService.GetOrders(keyword, status, paymentStatus, pageNumber, pageSize);

            return View(result);
        }

        ///////////////////////// DETAIL /////////////////////////
        [HttpGet]
        public async Task<IActionResult> Detail(string id) {
            var model = await adminOrderQueryService.GetOrderDetailAsync(id);

            if (model == null) {
                return NotFound();
            }

            return View(model);
        }

        ///////////////////////// UPDATE STATUS /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, OrderStatus status, PaymentStatus paymentStatus) {
            if (!ModelState.IsValid) {
                TempData["Error"] = "Invalid status request.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var changedBy = User.Identity?.Name ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Admin";
            var result = await orderStatusService.ChangeStatusAsync(id, status, paymentStatus, changedBy);

            if (!result.Succeeded) {
                if (result.Message == "Order not found.") {
                    return NotFound();
                }

                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Detail), new { id });
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
