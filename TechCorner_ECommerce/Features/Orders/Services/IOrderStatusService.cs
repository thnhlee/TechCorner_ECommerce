using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.Models.Enums;

namespace TechCorner_ECommerce.Services {
    public interface IOrderStatusService {
        IReadOnlyList<OrderStatus> GetNextStatuses(OrderStatus currentStatus);
        bool CanTransition(OrderStatus currentStatus, OrderStatus newStatus);
        string GetInvalidTransitionMessage(OrderStatus currentStatus, OrderStatus newStatus);
        Task<OrderStatusChangeResult> ChangeStatusAsync(string orderCode, OrderStatus newStatus, PaymentStatus paymentStatus, string changedBy);
    }

    public class OrderStatusChangeResult {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = "";
        public Order? Order { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public PaymentStatus OldPaymentStatus { get; set; }
        public PaymentStatus NewPaymentStatus { get; set; }
    }
}
