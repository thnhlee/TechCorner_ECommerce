namespace TechCorner_ECommerce.ViewModels {
    public class CartOperationResult {
        public List<CartItemVM> Cart { get; set; } = new();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public int? Available { get; set; }
        public bool Removed { get; set; }
        public int Quantity => Cart.Sum(p => p.Quantity);
        public decimal SubTotal => Cart.Sum(p => p.SubTotal);
    }
}
