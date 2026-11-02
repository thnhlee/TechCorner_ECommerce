using TechCorner_ECommerce.ViewModels;
using X.PagedList;

namespace TechCorner_ECommerce.Services {
    public interface IProductCatalogService {
        IPagedList<ProductVM> GetProducts(int? categoryId, string? keyword, int pageNumber, int pageSize);
        ProductVM? GetProductDetail(string slug);
        object? GetVariants(int productId);
    }
}
