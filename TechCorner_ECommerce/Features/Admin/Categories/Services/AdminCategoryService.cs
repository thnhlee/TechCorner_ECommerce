using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Services {
    public class AdminCategoryService : IAdminCategoryService {
        private readonly AppDbContext db;

        public AdminCategoryService(AppDbContext context) {
            db = context;
        }

        ///////////////////////// CATEGORY LIST /////////////////////////
        public CategoryIndexResult GetCategories(string? search, int pageNumber, int pageSize) {
            var query = db.Categories
                .Include(x => x.SubCategories)
                    .ThenInclude(x => x.ParentProducts)
                    .ThenInclude(x => x.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search)) {
                search = search.Trim();
                query = query.Where(x => x.Name.Contains(search));
            }

            return new CategoryIndexResult {
                Search = search,
                Items = query.Select(x => new MenuCategoryVM {
                    Id = x.CategoryId,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    ProductCount = x.SubCategories
                        .SelectMany(s => s.ParentProducts)
                        .SelectMany(p => p.Products)
                        .Count(),
                    SubCategories = x.SubCategories.Select(s => new MenuSubCategoryVM {
                        Id = s.Id,
                        Name = s.Name
                    }).ToList()
                }).ToPagedList(pageNumber, pageSize)
            };
        }

        ///////////////////////// CATEGORY CREATE /////////////////////////
        public AdminCategoryOperationResult CreateCategory(CategoryCreateVM vm) {
            var validation = ValidateCategoryName(vm.Name);

            if (!validation.Succeeded) {
                return validation;
            }

            var category = new Category {
                Name = vm.Name,
                CreatedAt = DateTime.Now
            };

            db.Categories.Add(category);
            db.SaveChanges();

            return AdminCategoryOperationResult.Success();
        }

        ///////////////////////// CATEGORY EDIT /////////////////////////
        public CategoryEditVM? GetCategoryEdit(int id) {
            return db.Categories
                .Where(x => x.CategoryId == id)
                .Select(x => new CategoryEditVM {
                    CategoryId = x.CategoryId,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefault();
        }

        public AdminCategoryOperationResult EditCategory(CategoryEditVM vm) {
            var validation = ValidateCategoryName(vm.Name, vm.CategoryId);

            if (!validation.Succeeded) {
                return validation;
            }

            var category = db.Categories.Find(vm.CategoryId);

            if (category == null) {
                return AdminCategoryOperationResult.Missing("Category not found");
            }

            category.Name = vm.Name;
            category.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            return AdminCategoryOperationResult.Success();
        }

        ///////////////////////// CATEGORY DELETE /////////////////////////
        public AdminCategoryOperationResult DeleteCategory(int id) {
            var category = db.Categories
                .Include(x => x.SubCategories)
                .ThenInclude(x => x.ParentProducts)
                .FirstOrDefault(x => x.CategoryId == id);

            if (category == null) {
                return AdminCategoryOperationResult.Failure("Category not found");
            }

            if (category.SubCategories.Any(x => x.ParentProducts.Any())) {
                return AdminCategoryOperationResult.Failure("This category cannot be deleted because it is already used.");
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            return AdminCategoryOperationResult.Success();
        }

        ///////////////////////// SUBCATEGORY LIST /////////////////////////
        public SubCategoryIndexResult GetSubCategories(int? categoryId, string? search, int pageNumber, int pageSize) {
            var query = db.SubCategories
                .Include(x => x.Category)
                .Include(x => x.ParentProducts)
                .AsQueryable();

            if (categoryId.HasValue) {
                query = query.Where(x => x.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(search)) {
                search = search.Trim();
                query = query.Where(x => x.Name.Contains(search));
            }

            return new SubCategoryIndexResult {
                CategoryId = categoryId,
                Search = search,
                Categories = GetCategoryEntities(),
                Items = query.Select(x => new MenuSubCategoryVM {
                    Id = x.Id,
                    Name = x.Name,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    Quantity = x.ParentProducts
                        .SelectMany(p => p.Products)
                        .Count(),
                }).ToPagedList(pageNumber, pageSize)
            };
        }

        ///////////////////////// SUBCATEGORY CREATE /////////////////////////
        public SubCategoryCreateVM GetSubCategoryCreateModel() {
            return new SubCategoryCreateVM {
                Categories = GetCategoryOptions()
            };
        }

        public AdminCategoryOperationResult CreateSubCategory(SubCategoryCreateVM vm) {
            var validation = ValidateSubCategory(vm.Name, vm.CategoryId);

            if (!validation.Succeeded) {
                return validation;
            }

            var subCategory = new SubCategory {
                Name = vm.Name,
                CategoryId = vm.CategoryId
            };

            db.SubCategories.Add(subCategory);
            db.SaveChanges();

            return AdminCategoryOperationResult.Success();
        }

        ///////////////////////// SUBCATEGORY EDIT /////////////////////////
        public SubCategoryEditVM? GetSubCategoryEdit(int id) {
            return db.SubCategories
                .Where(x => x.Id == id)
                .Select(x => new SubCategoryEditVM {
                    Id = x.Id,
                    Name = x.Name,
                    CategoryId = x.CategoryId
                })
                .FirstOrDefault();
        }

        public List<Category> GetCategoryEntities() {
            return db.Categories.ToList();
        }

        public AdminCategoryOperationResult EditSubCategory(SubCategoryEditVM vm) {
            var validation = ValidateSubCategory(vm.Name, vm.CategoryId, vm.Id);

            if (!validation.Succeeded) {
                return validation;
            }

            var subCategory = db.SubCategories.Find(vm.Id);

            if (subCategory == null) {
                return AdminCategoryOperationResult.Missing("Not found");
            }

            subCategory.Name = vm.Name;
            subCategory.CategoryId = vm.CategoryId;
            db.SaveChanges();

            return AdminCategoryOperationResult.Success();
        }

        ///////////////////////// SUBCATEGORY DELETE /////////////////////////
        public AdminCategoryOperationResult DeleteSubCategory(int id) {
            var subCategory = db.SubCategories
                .Include(x => x.ParentProducts)
                .FirstOrDefault(x => x.Id == id);

            if (subCategory == null) {
                return AdminCategoryOperationResult.Failure("Not found");
            }

            if (subCategory.ParentProducts.Any()) {
                return AdminCategoryOperationResult.Failure("This subcategory cannot be deleted because it is already used.");
            }

            db.SubCategories.Remove(subCategory);
            db.SaveChanges();

            return AdminCategoryOperationResult.Success();
        }

        ///////////////////////// ATTRIBUTE MANAGEMENT /////////////////////////
        public AttributeManageVM GetAttributeManageModel(int? categoryId) {
            var model = new AttributeManageVM {
                SelectedCategoryId = categoryId,
                Categories = GetCategoryOptions()
            };

            if (categoryId.HasValue) {
                model.Attributes = db.ProductAttributes
                    .Where(a => a.CategoryId == categoryId.Value)
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

            return model;
        }

        public AdminCategoryOperationResult AddAttribute(CreateAttributeVM model) {
            if (string.IsNullOrWhiteSpace(model.Name)) {
                return AdminCategoryOperationResult.Failure("Attribute name is required");
            }

            var name = model.Name.Trim();
            var exists = db.ProductAttributes.Any(x =>
                x.CategoryId == model.CategoryId &&
                x.Name.ToLower() == name.ToLower());

            if (exists) {
                return AdminCategoryOperationResult.Failure("Attribute already exists in this category");
            }

            var attr = new ProductAttribute {
                CategoryId = model.CategoryId,
                Name = name
            };

            db.ProductAttributes.Add(attr);
            db.SaveChanges();

            return AdminCategoryOperationResult.Success(attr.Id, attr.Name);
        }

        public AdminCategoryOperationResult AddAttributeValue(CreateAttributeValueVM model) {
            if (string.IsNullOrWhiteSpace(model.Value)) {
                return AdminCategoryOperationResult.Failure("Value is required");
            }

            var valueText = model.Value.Trim();
            var exists = db.AttributeValues.Any(x =>
                x.ProductAttributeId == model.AttributeId &&
                x.Value.ToLower() == valueText.ToLower());

            if (exists) {
                return AdminCategoryOperationResult.Failure("Value already exists");
            }

            var value = new AttributeValue {
                ProductAttributeId = model.AttributeId,
                Value = valueText
            };

            db.AttributeValues.Add(value);
            db.SaveChanges();

            return AdminCategoryOperationResult.Success(value.Id, value: value.Value);
        }

        public AdminCategoryOperationResult DeleteAttribute(int id) {
            var attr = db.ProductAttributes
                .Include(x => x.AttributeValues)
                .FirstOrDefault(x => x.Id == id);

            if (attr == null) {
                return AdminCategoryOperationResult.Failure("Attribute not found");
            }

            var attrValueIds = attr.AttributeValues.Select(v => v.Id);
            var isUsed = db.ProductAttributeValues.Any(x => attrValueIds.Contains(x.AttributeValueId));

            if (isUsed) {
                return AdminCategoryOperationResult.Failure("This attribute cannot be deleted because it is already used.");
            }

            db.AttributeValues.RemoveRange(attr.AttributeValues);
            db.ProductAttributes.Remove(attr);
            db.SaveChanges();

            return AdminCategoryOperationResult.Success();
        }

        public AdminCategoryOperationResult DeleteAttributeValue(int id) {
            var value = db.AttributeValues.FirstOrDefault(x => x.Id == id);

            if (value == null) {
                return AdminCategoryOperationResult.Failure("Value not found");
            }

            var isUsed = db.ProductAttributeValues.Any(x => x.AttributeValueId == id);

            if (isUsed) {
                return AdminCategoryOperationResult.Failure("This value cannot be deleted because it is already used.");
            }

            db.AttributeValues.Remove(value);
            db.SaveChanges();

            return AdminCategoryOperationResult.Success();
        }

        private AdminCategoryOperationResult ValidateCategoryName(string name, int? excludedCategoryId = null) {
            if (string.IsNullOrWhiteSpace(name)) {
                return AdminCategoryOperationResult.Failure("Category name is required", "Name");
            }

            var normalizedName = name.Trim().ToLower();
            var exists = db.Categories.Any(x =>
                (!excludedCategoryId.HasValue || x.CategoryId != excludedCategoryId.Value) &&
                x.Name.Trim().ToLower() == normalizedName);

            if (exists) {
                return AdminCategoryOperationResult.Failure("Category already exists", "Name");
            }

            return AdminCategoryOperationResult.Success();
        }

        private AdminCategoryOperationResult ValidateSubCategory(string name, int categoryId, int? excludedSubCategoryId = null) {
            if (string.IsNullOrWhiteSpace(name)) {
                return AdminCategoryOperationResult.Failure("SubCategory name is required", "Name");
            }

            if (categoryId == 0) {
                return AdminCategoryOperationResult.Failure("Please select category", "CategoryId");
            }

            var trimmedName = name.Trim();
            var exists = db.SubCategories.Any(x =>
                (!excludedSubCategoryId.HasValue || x.Id != excludedSubCategoryId.Value) &&
                x.Name.Trim() == trimmedName &&
                x.CategoryId == categoryId);

            if (exists) {
                return AdminCategoryOperationResult.Failure("SubCategory already exists in this category", "Name");
            }

            return AdminCategoryOperationResult.Success();
        }

        private List<CategoryVM> GetCategoryOptions() {
            return db.Categories
                .Select(x => new CategoryVM {
                    Id = x.CategoryId,
                    Name = x.Name
                })
                .ToList();
        }
    }
}
