using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechCorner_ECommerce.Models {
    public class Product {
        public int Id { get; set; }
        public string SkuCode { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }

        [Required]
        public int ParentProductId { get; set; }
        public ParentProduct ParentProduct { get; set; } = null!;

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
    }
}
