using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechCorner_ECommerce.Models {
    public class Product {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        public int ParentProductId { get; set; }
        public ParentProduct ParentProduct { get; set; }

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; }
    }
}