using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.Services {
    public class ProductImageService : IProductImageService {
        private readonly AppDbContext db;
        private readonly IWebHostEnvironment webHost;

        public ProductImageService(AppDbContext context, IWebHostEnvironment webHostEnvironment) {
            db = context;
            webHost = webHostEnvironment;
        }

        public async Task AddImagesAsync(int parentProductId, IEnumerable<IFormFile>? files, bool firstImageIsPrimary) {
            if (files == null || !files.Any()) {
                return;
            }

            var uploadPath = Path.Combine(webHost.WebRootPath, "images");
            Directory.CreateDirectory(uploadPath);

            var isPrimary = firstImageIsPrimary;

            foreach (var file in files) {
                if (file == null || file.Length == 0) {
                    continue;
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var path = Path.Combine(uploadPath, fileName);

                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                db.ProductImages.Add(new ProductImage {
                    ParentProductId = parentProductId,
                    ImageUrl = "/images/" + fileName,
                    IsPrimary = isPrimary
                });

                isPrimary = false;
            }
        }

        public void DeleteImageFile(ProductImage image) {
            if (string.IsNullOrWhiteSpace(image.ImageUrl)) {
                return;
            }

            var relativePath = image.ImageUrl.TrimStart('/');
            var fullPath = Path.Combine(
                webHost.WebRootPath,
                relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(fullPath)) {
                File.Delete(fullPath);
            }
        }
    }
}
