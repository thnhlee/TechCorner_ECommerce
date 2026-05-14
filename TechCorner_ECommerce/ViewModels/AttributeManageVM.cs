namespace TechCorner_ECommerce.ViewModels {
    public class AttributeManageVM {
        public List<CategoryVM> Categories { get; set; } = new();

        public int? SelectedCategoryId { get; set; }

        public List<ProductAttributeVM> Attributes { get; set; } = new();
    }

    public class CreateAttributeVM {
        public int CategoryId { get; set; }

        public string Name { get; set; }
    }

    public class CreateAttributeValueVM {
        public int AttributeId { get; set; }

        public string Value { get; set; }
    }
}