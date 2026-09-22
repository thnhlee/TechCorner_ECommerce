using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Helpers;

namespace TechCorner_ECommerce.Services {
    public class UniqueCodeService : IUniqueCodeService {
        private readonly AppDbContext db;

        public UniqueCodeService(AppDbContext context) {
            db = context;
        }

        public string CreateParentProductPublicId() {
            var publicId = CodeGenerator.Generate("PROD");

            while (db.ParentProducts.Any(x => x.PublicId == publicId)) {
                publicId = CodeGenerator.Generate("PROD");
            }

            return publicId;
        }

        public string CreateSkuCode() {
            var sku = CodeGenerator.Generate("SKU");

            while (db.Products.Any(x => x.SkuCode == sku)) {
                sku = CodeGenerator.Generate("SKU");
            }

            return sku;
        }

        public string CreateOrderCode() {
            var orderCode = CodeGenerator.Generate("ORD");

            while (db.Orders.Any(x => x.OrderCode == orderCode)) {
                orderCode = CodeGenerator.Generate("ORD");
            }

            return orderCode;
        }
    }
}
