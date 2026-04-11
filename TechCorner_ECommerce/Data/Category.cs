using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Data {
    public class Category {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Navigation
        public List<SubCategory> SubCategories { get; set; } = new();
        public List<Product> Products { get; set; } = new();
    }
}
