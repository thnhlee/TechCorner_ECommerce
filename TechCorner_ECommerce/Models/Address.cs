using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class Address {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }


        [StringLength(300)]
        public string? FullAddress { get; set; }


        [StringLength(100)]
        public string? ReceiverName { get; set; }


        [Phone]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
