namespace TechCorner_ECommerce.Models {
    public class ProductAttributeValue {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int AttributeValueId { get; set; }
        public AttributeValue AttributeValue { get; set; }
    }
}
