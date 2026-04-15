namespace TechCorner_ECommerce.ViewModels {
    public class CartItemVM {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double SubTotal => Price * Quantity;
    }
}
