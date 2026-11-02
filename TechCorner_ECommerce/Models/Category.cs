using System.ComponentModel.DataAnnotations;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Models {
    public class Category {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
    }
}
