using System.ComponentModel.DataAnnotations;
using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.ViewModels {
    public class CreateProductVM {
        public string Name { get; set; }
        public string Description { get; set; }
        public int SubCategoryId { get; set; }

        public List<IFormFile>? Images { get; set; }
        
        public List<ProductAttributeVM> Attributes { get; set; } = new();
        public List<CategoryVM> Categories { get; set; } = new();
        public List<SubCategoryVM> SubCategories { get; set; } = new();
        public List<ProductVariantVM> Variants { get; set; } = new();
    }

    public class ProductVariantVM {
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public List<int> AttributeValueIds { get; set; }
    }

    public class ProductAttributeVM {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public List<AttributeValueVM> Values { get; set; }
    }

    public class AttributeValueVM {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class CategoryVM {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SubCategoryVM {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
    }

}
