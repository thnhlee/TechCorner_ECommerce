using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Services {
    public interface IProductImageService {
        Task AddImagesAsync(int parentProductId, IEnumerable<IFormFile>? files, bool firstImageIsPrimary);
        void DeleteImageFile(ProductImage image);
    }
}
