using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models.Enums;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class OrderController : Controller {
        private readonly AppDbContext db;
        private readonly IOrderStatusService orderStatusService;

        public OrderController(
            AppDbContext context,
            IOrderStatusService orderStatusService) {
            db = context;
            this.orderStatusService = orderStatusService;
        }

        public IActionResult Index(string? keyword, OrderStatus? status, PaymentStatus? paymentStatus, int? page) {
            const int pageSize = 8;
            var pageNumber = page ?? 1;

            ViewBag.SearchQuery = keyword;
            ViewBag.Status = status;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.OrderStatuses = Enum.GetValues<OrderStatus>();
            ViewBag.PaymentStatuses = Enum.GetValues<PaymentStatus>();

            var orders = db.Orders
                .Include(x => x.Payments)
                .Include(x => x.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword)) {
                keyword = keyword.Trim();

                orders = orders.Where(x =>
                    EF.Functions.Like(EF.Functions.Collate(x.OrderCode, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%") ||
                    EF.Functions.Like(EF.Functions.Collate(x.ReceiverName, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%") ||
                    EF.Functions.Like(EF.Functions.Collate(x.ReceiverEmail, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%") ||
                    EF.Functions.Like(EF.Functions.Collate(x.ReceiverPhone, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%"));
            }

            if (status.HasValue) {
                orders = orders.Where(x => x.Status == status.Value);
            }

            if (paymentStatus.HasValue) {
                orders = orders.Where(x => x.Payments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => p.Status)
                    .FirstOrDefault() == paymentStatus.Value);
            }

            var result = orders
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new AdminOrderVM {
                    Id = x.Id,
                    OrderCode = x.OrderCode,
                    OrderDate = x.OrderDate,
                    ReceiverName = x.ReceiverName,
                    ReceiverEmail = x.ReceiverEmail,
                    ReceiverPhone = x.ReceiverPhone,
                    TotalPrice = x.TotalPrice,
                    Status = x.Status,
                    PaymentMethod = x.Payments
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => p.PaymentMethod)
                        .FirstOrDefault() ?? "",
                    PaymentStatus = x.Payments
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => p.Status)
                        .FirstOrDefault(),
                    ItemCount = x.OrderDetails.Sum(d => d.Quantity)
                })
                .ToPagedList(pageNumber, pageSize);

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id) {
            var order = await db.Orders
                .Include(x => x.Payments)
                .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Product!)
                    .ThenInclude(x => x.ParentProduct)
                    .ThenInclude(x => x.Images)
                .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Product!)
                    .ThenInclude(x => x.ProductAttributeValues)
                    .ThenInclude(x => x.AttributeValue)
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(x => x.OrderCode == id);

            if (order == null) {
                return NotFound();
            }

            var payment = order.Payments
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            var model = new AdminOrderDetailVM {
                OrderCode = order.OrderCode,
                OrderDate = order.OrderDate,
                ReceiverName = order.ReceiverName,
                ReceiverEmail = order.ReceiverEmail,
                ReceiverPhone = order.ReceiverPhone,
                ShippingAddress = order.ShippingAddress,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                PaymentMethod = payment?.PaymentMethod ?? "N/A",
                PaymentStatus = payment?.Status ?? PaymentStatus.Pending,
                OrderStatuses = Enum.GetValues<OrderStatus>().ToList(),
                AvailableNextStatuses = orderStatusService.GetNextStatuses(order.Status).ToList(),
                PaymentStatuses = Enum.GetValues<PaymentStatus>().ToList(),
                StatusHistories = order.StatusHistories
                    .OrderByDescending(x => x.ChangedAt)
                    .Select(x => new AdminOrderStatusHistoryVM {
                        OldStatus = x.OldStatus,
                        NewStatus = x.NewStatus,
                        ChangedBy = x.ChangedBy,
                        ChangedAt = x.ChangedAt,
                        Note = x.Note
                    }).ToList(),
                Items = order.OrderDetails.Select(item => {
                    var product = item.Product;
                    var parent = product?.ParentProduct;

                    var imageUrl = parent?.Images
                        .Where(x => x.IsPrimary)
                        .Select(x => x.ImageUrl)
                        .FirstOrDefault()
                        ?? parent?.Images
                            .Select(x => x.ImageUrl)
                            .FirstOrDefault()
                        ?? "";

                    var attributesText = product?.ProductAttributeValues?
                        .Select(x => x.AttributeValue?.Value)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();

                    return new AdminOrderDetailItemVM {
                        ProductName = parent?.Name ?? "Product unavailable",
                        ProductImageUrl = imageUrl,
                        SkuCode = product?.SkuCode ?? "",
                        AttributesText = attributesText == null ? "" : string.Join(" / ", attributesText),
                        Price = item.Price,
                        Quantity = item.Quantity,
                        LineTotal = item.Price * item.Quantity
                    };
                }).ToList()
            };

            return View(model);
        }

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
