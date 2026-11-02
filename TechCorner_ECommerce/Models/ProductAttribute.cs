using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.Models {
    public class ProductAttribute {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
    }
}
