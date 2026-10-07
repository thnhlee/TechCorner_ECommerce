using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IAdminCategoryService {
        CategoryIndexResult GetCategories(string? search, int pageNumber, int pageSize);
        AdminCategoryOperationResult CreateCategory(CategoryCreateVM vm);
        CategoryEditVM? GetCategoryEdit(int id);
        AdminCategoryOperationResult EditCategory(CategoryEditVM vm);
        AdminCategoryOperationResult DeleteCategory(int id);

        SubCategoryIndexResult GetSubCategories(int? categoryId, string? search, int pageNumber, int pageSize);
        SubCategoryCreateVM GetSubCategoryCreateModel();
        AdminCategoryOperationResult CreateSubCategory(SubCategoryCreateVM vm);
        SubCategoryEditVM? GetSubCategoryEdit(int id);
        List<Models.Category> GetCategoryEntities();
        AdminCategoryOperationResult EditSubCategory(SubCategoryEditVM vm);
        AdminCategoryOperationResult DeleteSubCategory(int id);

        AttributeManageVM GetAttributeManageModel(int? categoryId);
        AdminCategoryOperationResult AddAttribute(CreateAttributeVM model);
        AdminCategoryOperationResult AddAttributeValue(CreateAttributeValueVM model);
        AdminCategoryOperationResult DeleteAttribute(int id);
        AdminCategoryOperationResult DeleteAttributeValue(int id);
    }
}
