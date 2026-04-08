namespace TechCorner_ECommerce.Data {
    public class Order {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Pending";

        public User User { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
