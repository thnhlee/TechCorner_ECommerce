using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class CartItem {
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        [Required]
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
