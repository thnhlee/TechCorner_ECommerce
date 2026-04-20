using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class AttributeValue {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Value { get; set; }

        [Required]
        public int AttributeId { get; set; }

        public ProductAttribute Attribute { get; set; }

        public ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = new List<VariantAttributeValue>();
    }
}
