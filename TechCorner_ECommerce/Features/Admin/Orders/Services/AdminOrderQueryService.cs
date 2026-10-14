using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models.Enums;
using TechCorner_ECommerce.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Services {
    public class AdminOrderQueryService : IAdminOrderQueryService {
        private readonly AppDbContext db;
        private readonly IOrderStatusService orderStatusService;

        public AdminOrderQueryService(AppDbContext context, IOrderStatusService orderStatusService) {
            db = context;
            this.orderStatusService = orderStatusService;
        }

        ///////////////////////// ORDER LIST /////////////////////////
        public IPagedList<AdminOrderVM> GetOrders(string? keyword, OrderStatus? status, PaymentStatus? paymentStatus, int pageNumber, int pageSize) {
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

            return orders
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
        }

        ///////////////////////// ORDER DETAIL /////////////////////////
        public async Task<AdminOrderDetailVM?> GetOrderDetailAsync(string orderCode) {
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
                .FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            if (order == null) {
                return null;
            }

            var payment = order.Payments
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            // Product image and attributes are projected here so the Razor view stays display-only.
            return new AdminOrderDetailVM {
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
        }
    }
}
