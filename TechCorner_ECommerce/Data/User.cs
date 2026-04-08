namespace TechCorner_ECommerce.Data {
    public class User {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Customer"; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<Order> Orders { get; set; } = new();
    }
}
