using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class ApplicationUser : IdentityUser {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        
    }
}
