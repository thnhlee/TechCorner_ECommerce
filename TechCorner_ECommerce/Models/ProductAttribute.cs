using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class ProductAttribute {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<AttributeValue> Values { get; set; }
    }
}
