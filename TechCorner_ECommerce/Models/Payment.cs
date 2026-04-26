using System.ComponentModel.DataAnnotations;
using TechCorner_ECommerce.Models.Enums;
namespace TechCorner_ECommerce.Models {
    public class Payment {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        public Order Order { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
