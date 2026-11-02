using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public class CatalogLookupService : ICatalogLookupService {
        private readonly AppDbContext db;

        public CatalogLookupService(AppDbContext context) {
            db = context;
        }

        public List<CategoryVM> GetCategories() {
            return db.Categories
                .Select(c => new CategoryVM {
                    Id = c.CategoryId,
                    Name = c.Name
                })
                .ToList();
        }

        public List<SubCategoryVM> GetSubCategories() {
            return db.SubCategories
                .Select(s => new SubCategoryVM {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId
                })
                .ToList();
        }

        public List<ProductAttributeVM> GetAttributes() {
            return db.ProductAttributes
                .Include(a => a.AttributeValues)
                .Select(a => new ProductAttributeVM {
                    Id = a.Id,
                    Name = a.Name,
                    CategoryId = a.CategoryId,
                    Values = a.AttributeValues.Select(v => new AttributeValueVM {
                        Id = v.Id,
                        Value = v.Value
                    }).ToList()
                })
                .ToList();
        }

        public void PopulateProductLookups(CreateProductVM model) {
            model.Categories = GetCategories();
            model.SubCategories = GetSubCategories();
            model.Attributes = GetAttributes();
        }

        public void PopulateProductLookups(EditProductVM model) {
            model.Categories = GetCategories();
            model.SubCategories = GetSubCategories();
            model.Attributes = GetAttributes();
        }
    }
}
