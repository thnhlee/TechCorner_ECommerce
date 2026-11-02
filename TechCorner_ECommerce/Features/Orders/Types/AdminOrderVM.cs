using TechCorner_ECommerce.Models.Enums;

namespace TechCorner_ECommerce.ViewModels {
    public class AdminOrderVM {
        public int Id { get; set; }
        public string OrderCode { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public string ReceiverName { get; set; } = "";
        public string ReceiverEmail { get; set; } = "";
        public string ReceiverPhone { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public string PaymentMethod { get; set; } = "";
        public PaymentStatus PaymentStatus { get; set; }
        public int ItemCount { get; set; }
    }

    public class AdminOrderDetailVM {
        public string OrderCode { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public string ReceiverName { get; set; } = "";
        public string ReceiverEmail { get; set; } = "";
        public string ReceiverPhone { get; set; } = "";
        public string ShippingAddress { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public string PaymentMethod { get; set; } = "";
        public PaymentStatus PaymentStatus { get; set; }
        public List<OrderStatus> OrderStatuses { get; set; } = new();
        public List<OrderStatus> AvailableNextStatuses { get; set; } = new();
        public List<PaymentStatus> PaymentStatuses { get; set; } = new();
        public List<AdminOrderStatusHistoryVM> StatusHistories { get; set; } = new();
        public List<AdminOrderDetailItemVM> Items { get; set; } = new();
        public bool IsTerminalStatus => Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled;
    }

    public class AdminOrderDetailItemVM {
        public string ProductName { get; set; } = "";
        public string ProductImageUrl { get; set; } = "";
        public string SkuCode { get; set; } = "";
        public string AttributesText { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class AdminOrderStatusHistoryVM {
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string ChangedBy { get; set; } = "";
        public DateTime ChangedAt { get; set; }
        public string? Note { get; set; }
    }
}
