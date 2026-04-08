using System.ComponentModel.DataAnnotations.Schema;

namespace TechCorner_ECommerce.Data {
    public class Product {
        public int Id { get; set; }

        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
