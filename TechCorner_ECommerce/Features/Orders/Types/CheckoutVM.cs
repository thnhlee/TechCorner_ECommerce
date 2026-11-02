using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.ViewModels {
    public class CheckoutVM {
        [Required(ErrorMessage = "Receiver name is required")]
        [StringLength(100)]
        public string? ReceiverName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(256)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(300)]
        public string? FullAddress { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "COD";

        public List<CartItemVM> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(x => x.SubTotal);
        public decimal DeliveryFee { get; set; }
        public decimal Total => SubTotal + DeliveryFee;
    }

    public class OrderListVM {
        public int Id { get; set; }
        public string OrderCode { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
    }
}
