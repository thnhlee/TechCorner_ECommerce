namespace TechCorner_ECommerce.ViewModels {
    public class VariantVM {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public List<AttributeVM> Attributes { get; set; }
    }
}
