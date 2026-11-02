using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface ICatalogLookupService {
        List<CategoryVM> GetCategories();
        List<SubCategoryVM> GetSubCategories();
        List<ProductAttributeVM> GetAttributes();
        void PopulateProductLookups(CreateProductVM model);
        void PopulateProductLookups(EditProductVM model);
    }
}
