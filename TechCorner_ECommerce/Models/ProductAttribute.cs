using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class ProductAttribute {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
    }
}
