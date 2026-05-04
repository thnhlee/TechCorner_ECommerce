using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class AttributeValue {
        public int Id { get; set; }
        public string Value { get; set; }

        public int ProductAttributeId { get; set; }
        public ProductAttribute ProductAttribute { get; set; }

        public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; }
    }
}
