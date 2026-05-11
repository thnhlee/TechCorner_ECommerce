namespace TechCorner_ECommerce.ViewModels {

    public class EditProductVM {

        public int ParentProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int SubCategoryId { get; set; }

        public List<IFormFile>? NewImages { get; set; }

        public List<ProductImageVM> ExistingImages { get; set; } = new();

        public List<ProductVariantEditVM> Variants { get; set; } = new();

        public List<ProductAttributeVM> Attributes { get; set; } = new();

        public List<CategoryVM> Categories { get; set; } = new();

        public List<SubCategoryVM> SubCategories { get; set; } = new();
    }

    public class ProductImageVM {

        public int Id { get; set; }

        public string ImageUrl { get; set; }
    }

    public class ProductVariantEditVM {

        public int ProductId { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public List<int> AttributeValueIds { get; set; } = new();
    }
}
