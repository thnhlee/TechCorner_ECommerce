using TechCorner_ECommerce.Models.Enums;

namespace TechCorner_ECommerce.Models {
    public class OrderStatusHistory {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string ChangedBy { get; set; } = "";
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        public string? Note { get; set; }
    }
}
