using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IAdminProductService {
        Task<AdminProductOperationResult> AddProductAsync(CreateProductVM model);
        EditProductVM? GetEditProduct(string publicId);
        Task<AdminProductOperationResult> EditProductAsync(EditProductVM model);
        AdminProductOperationResult DeleteImage(int id);
        AdminProductOperationResult DeleteVariant(int id);
    }
}
