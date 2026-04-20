namespace TechCorner_ECommerce.Models {
    public class VariantAttributeValue {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }

        public int AttributeValueId { get; set; }
        public AttributeValue AttributeValue { get; set; }
    }
}
