using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.Models.Enums;

namespace TechCorner_ECommerce.Services {
    public class OrderStatusService : IOrderStatusService {
        private static readonly IReadOnlyDictionary<OrderStatus, IReadOnlyList<OrderStatus>> AllowedTransitions =
            new Dictionary<OrderStatus, IReadOnlyList<OrderStatus>> {
                [OrderStatus.Pending] = new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
                [OrderStatus.Confirmed] = new[] { OrderStatus.Shipping, OrderStatus.Cancelled },
                [OrderStatus.Shipping] = new[] { OrderStatus.Delivered },
                [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
                [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
            };

        private readonly AppDbContext db;

        public OrderStatusService(AppDbContext context) {
            db = context;
        }

        public IReadOnlyList<OrderStatus> GetNextStatuses(OrderStatus currentStatus) {
            return AllowedTransitions.TryGetValue(currentStatus, out var nextStatuses)
                ? nextStatuses
                : Array.Empty<OrderStatus>();
        }

        public bool CanTransition(OrderStatus currentStatus, OrderStatus newStatus) {
            return GetNextStatuses(currentStatus).Contains(newStatus);
        }

        public string GetInvalidTransitionMessage(OrderStatus currentStatus, OrderStatus newStatus) {
            if (currentStatus == newStatus) {
                return $"Order is already {currentStatus}.";
            }

            if (currentStatus == OrderStatus.Delivered) {
                return "Delivered orders are final and cannot be changed.";
            }

            if (currentStatus == OrderStatus.Cancelled) {
                return "Cancelled orders are final and cannot be changed.";
            }

            return $"Cannot change order status from {currentStatus} to {newStatus}.";
        }

        public async Task<OrderStatusChangeResult> ChangeStatusAsync(
            string orderCode,
            OrderStatus newStatus,
            PaymentStatus paymentStatus,
            string changedBy) {
            if (!Enum.IsDefined(typeof(OrderStatus), newStatus)) {
                return new OrderStatusChangeResult {
                    Succeeded = false,
                    Message = "Invalid order status."
                };
            }

            if (!Enum.IsDefined(typeof(PaymentStatus), paymentStatus)) {
                return new OrderStatusChangeResult {
                    Succeeded = false,
                    Message = "Invalid payment status."
                };
            }

            var order = await db.Orders
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            if (order == null) {
                return new OrderStatusChangeResult {
                    Succeeded = false,
                    Message = "Order not found."
                };
            }

            var oldStatus = order.Status;
            var payment = order.Payments
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();
            var oldPaymentStatus = payment?.Status ?? PaymentStatus.Pending;

            if (!CanTransition(oldStatus, newStatus)) {
                return new OrderStatusChangeResult {
                    Succeeded = false,
                    Message = GetInvalidTransitionMessage(oldStatus, newStatus),
                    Order = order,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    OldPaymentStatus = oldPaymentStatus,
                    NewPaymentStatus = paymentStatus
                };
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;

            if (payment != null) {
                payment.Status = paymentStatus;
                payment.PaidAt = paymentStatus == PaymentStatus.Paid
                    ? DateTime.Now
                    : null;
            }

            db.OrderStatusHistories.Add(new OrderStatusHistory {
                OrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? "System" : changedBy,
                ChangedAt = DateTime.Now
            });

            await db.SaveChangesAsync();

            return new OrderStatusChangeResult {
                Succeeded = true,
                Message = "Order status updated.",
                Order = order,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                OldPaymentStatus = oldPaymentStatus,
                NewPaymentStatus = paymentStatus
            };
        }
    }
}
