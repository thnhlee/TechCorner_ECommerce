using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class Address {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(300)]
        public string FullAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string ReceiverName { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
