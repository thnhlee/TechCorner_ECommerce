using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class ProductImage {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Required]
        [Url]
        public string ImageUrl { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
