using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Services {
    public class ProductCatalogService : IProductCatalogService {
        private readonly AppDbContext db;

        public ProductCatalogService(AppDbContext context) {
            db = context;
        }

        ///////////////////////// PRODUCT LIST /////////////////////////
        public IPagedList<ProductVM> GetProducts(int? categoryId, string? keyword, int pageNumber, int pageSize) {
            var products = db.ParentProducts
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Images)
                .Include(p => p.Products)
                .AsQueryable();

            if (categoryId.HasValue) {
                products = products.Where(p => p.SubCategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(keyword)) {
                // Search is case-insensitive and accent-insensitive through SQL collation.
                products = products.Where(p =>
                    EF.Functions.Like(
                        EF.Functions.Collate(p.Name, "SQL_Latin1_General_CP1_CI_AI"),
                        $"%{keyword}%"));
            }

            return products
                .OrderByDescending(p => p.Id)
                .Select(p => new ProductVM {
                    Id = p.Id,
                    Slug = p.Slug,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Products.Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                        ?? p.Images
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()
                        ?? "",
                    CategoryName = p.SubCategory.Category.Name
                })
                .ToPagedList(pageNumber, pageSize);
        }

        ///////////////////////// PRODUCT DETAIL /////////////////////////
        public ProductVM? GetProductDetail(string slug) {
            return db.ParentProducts
                .Where(p => p.Slug == slug)
                .Select(p => new ProductVM {
                    Id = p.Id,
                    Slug = p.Slug,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Products.Min(v => (decimal?)v.Price) ?? 0,
                    ImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                        ?? p.Images
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()
                        ?? "",
                    CategoryName = p.SubCategory.Category.Name,
                    SubCategoryName = p.SubCategory.Name,
                    Stock = p.Products.Sum(v => v.StockQuantity),
                    Variants = p.Products.Select(v => new VariantVM {
                        Id = v.Id,
                        Price = v.Price,
                        Stock = v.StockQuantity,
                        Attributes = v.ProductAttributeValues
                            .Select(a => new AttributeVM {
                                Name = a.AttributeValue.ProductAttribute != null
                                    ? a.AttributeValue.ProductAttribute.Name
                                    : "",
                                Value = a.AttributeValue.Value
                            })
                            .ToList()
                    }).ToList()
                })
                .SingleOrDefault();
        }

        ///////////////////////// VARIANT POPUP DATA /////////////////////////
        public object? GetVariants(int productId) {
            var product = db.ParentProducts
                .Include(p => p.Images)
                .Include(p => p.Products)
                    .ThenInclude(sku => sku.ProductAttributeValues)
                        .ThenInclude(pav => pav.AttributeValue)
                            .ThenInclude(av => av.ProductAttribute)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null) {
                return null;
            }

            return new {
                product.Id,
                product.Name,
                Image = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                Variants = product.Products.Select(v => new {
                    v.Id,
                    v.Price,
                    v.StockQuantity,
                    Attributes = v.ProductAttributeValues.Select(a => new {
                        Name = a.AttributeValue.ProductAttribute.Name,
                        Value = a.AttributeValue.Value
                    })
                })
            };
        }
    }
}
