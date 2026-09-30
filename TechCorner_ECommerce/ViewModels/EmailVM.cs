namespace TechCorner_ECommerce.ViewModels {
    public class OrderConfirmationEmailVM {
        public string ReceiverName { get; set; } = "";
        public string OrderCode { get; set; } = "";
        public string ShippingAddress { get; set; } = "";
        public string ReceiverPhone { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public List<OrderConfirmationEmailItemVM> Items { get; set; } = new();
    }

    public class OrderConfirmationEmailItemVM {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal => Price * Quantity;
    }
}
