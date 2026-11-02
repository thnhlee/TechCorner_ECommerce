using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public class AdminProductService : IAdminProductService {
        private readonly AppDbContext db;
        private readonly ISlugService slugService;
        private readonly ICatalogLookupService catalogLookup;
        private readonly IProductImageService productImageService;
        private readonly IUniqueCodeService uniqueCodeService;

        public AdminProductService(
            AppDbContext context,
            ISlugService slugService,
            ICatalogLookupService catalogLookup,
            IProductImageService productImageService,
            IUniqueCodeService uniqueCodeService) {
            db = context;
            this.slugService = slugService;
            this.catalogLookup = catalogLookup;
            this.productImageService = productImageService;
            this.uniqueCodeService = uniqueCodeService;
        }

        ///////////////////////// CREATE PRODUCT /////////////////////////
        public async Task<AdminProductOperationResult> AddProductAsync(CreateProductVM model) {
            var normalizedName = model.Name.Trim().ToLower();
            var exists = db.ParentProducts.Any(x =>
                x.SubCategoryId == model.SubCategoryId &&
                x.Name.Trim().ToLower() == normalizedName);

            if (exists) {
                return AdminProductOperationResult.Failure("Product already exists in this subcategory", "Name");
            }

            await using var transaction = await db.Database.BeginTransactionAsync();

            try {
                // Parent product stores shared display data; Product rows store sellable variants.
                var parent = new ParentProduct {
                    PublicId = uniqueCodeService.CreateParentProductPublicId(),
                    Name = model.Name.Trim(),
                    Slug = slugService.CreateSlug(model.Name),
                    Description = model.Description,
                    SubCategoryId = model.SubCategoryId
                };

                db.ParentProducts.Add(parent);
                await db.SaveChangesAsync();

                await productImageService.AddImagesAsync(parent.Id, model.Images, firstImageIsPrimary: true);

                if (model.Variants != null) {
                    foreach (var variant in model.Variants) {
                        if (variant.AttributeValueIds == null || !variant.AttributeValueIds.Any()) {
                            continue;
                        }

                        var product = new Product {
                            SkuCode = uniqueCodeService.CreateSkuCode(),
                            ParentProductId = parent.Id,
                            Price = variant.Price,
                            StockQuantity = variant.StockQuantity
                        };

                        db.Products.Add(product);
                        await db.SaveChangesAsync();

                        var values = variant.AttributeValueIds.Select(attrId =>
                            new ProductAttributeValue {
                                ProductId = product.Id,
                                AttributeValueId = attrId
                            });

                        db.ProductAttributeValues.AddRange(values);
                    }
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return AdminProductOperationResult.Success();
            }
            catch (Exception ex) {
                await transaction.RollbackAsync();
                return AdminProductOperationResult.Failure(ex.Message);
            }
        }

        public EditProductVM? GetEditProduct(string publicId) {
            var product = db.ParentProducts
                .Include(x => x.SubCategory)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Images)
                .Include(x => x.Products)
                    .ThenInclude(x => x.ProductAttributeValues)
                    .ThenInclude(x => x.AttributeValue)
                .FirstOrDefault(x => x.PublicId == publicId);

            if (product == null) {
                return null;
            }

            return new EditProductVM {
                ParentProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                SubCategoryId = product.SubCategoryId,
                Categories = catalogLookup.GetCategories(),
                SubCategories = catalogLookup.GetSubCategories(),
                Attributes = catalogLookup.GetAttributes(),
                ExistingImages = product.Images
                    .Select(i => new ProductImageVM {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl
                    })
                    .ToList(),
                Variants = product.Products
                    .Select(p => new ProductVariantEditVM {
                        ProductId = p.Id,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        AttributeValueIds = p.ProductAttributeValues
                            .Select(v => v.AttributeValueId)
                            .ToList()
                    })
                    .ToList()
            };
        }

        ///////////////////////// EDIT PRODUCT /////////////////////////
        public async Task<AdminProductOperationResult> EditProductAsync(EditProductVM model) {
            var parent = db.ParentProducts
                .Include(x => x.Products)
                .FirstOrDefault(x => x.Id == model.ParentProductId);

            if (parent == null) {
                return AdminProductOperationResult.Missing("Product not found");
            }

            parent.Name = model.Name.Trim();
            parent.Description = model.Description;
            parent.SubCategoryId = model.SubCategoryId;

            if (model.Variants != null) {
                var existingVariantKeys = db.Products
                    .Where(p => p.ParentProductId == parent.Id && !p.IsDeleted)
                    .Include(p => p.ProductAttributeValues)
                    .AsEnumerable()
                    .Select(p => string.Join(",", p.ProductAttributeValues
                        .Select(x => x.AttributeValueId)
                        .OrderBy(x => x)))
                    .ToHashSet();

                foreach (var variant in model.Variants) {
                    if (variant.ProductId > 0) {
                        var product = db.Products.Find(variant.ProductId);

                        if (product == null) {
                            continue;
                        }

                        product.Price = variant.Price;
                        product.StockQuantity = variant.StockQuantity;
                    }
                    else {
                        if (variant.AttributeValueIds == null || !variant.AttributeValueIds.Any()) {
                            continue;
                        }

                        var newVariantKey = string.Join(",", variant.AttributeValueIds.OrderBy(x => x));

                        // Prevent adding the same attribute combination twice.
                        if (existingVariantKeys.Contains(newVariantKey)) {
                            continue;
                        }

                        var newProduct = new Product {
                            ParentProductId = parent.Id,
                            SkuCode = uniqueCodeService.CreateSkuCode(),
                            Price = variant.Price,
                            StockQuantity = variant.StockQuantity
                        };

                        db.Products.Add(newProduct);
                        await db.SaveChangesAsync();

                        var values = variant.AttributeValueIds.Select(attrId =>
                            new ProductAttributeValue {
                                ProductId = newProduct.Id,
                                AttributeValueId = attrId
                            });

                        db.ProductAttributeValues.AddRange(values);
                        existingVariantKeys.Add(newVariantKey);
                    }
                }
            }

            await productImageService.AddImagesAsync(parent.Id, model.NewImages, firstImageIsPrimary: false);
            await db.SaveChangesAsync();

            return AdminProductOperationResult.Success();
        }

        ///////////////////////// DELETE IMAGE /////////////////////////
        public AdminProductOperationResult DeleteImage(int id) {
            var image = db.ProductImages.FirstOrDefault(x => x.Id == id);

            if (image == null) {
                return AdminProductOperationResult.Failure("Image not found");
            }

            productImageService.DeleteImageFile(image);
            db.ProductImages.Remove(image);
            db.SaveChanges();

            return AdminProductOperationResult.Success();
        }

        ///////////////////////// DELETE VARIANT /////////////////////////
        public AdminProductOperationResult DeleteVariant(int id) {
            var product = db.Products
                .IgnoreQueryFilters()
                .FirstOrDefault(x => x.Id == id);

            if (product == null) {
                return AdminProductOperationResult.Failure("Variant not found");
            }

            var parent = db.ParentProducts
                .IgnoreQueryFilters()
                .FirstOrDefault(x => x.Id == product.ParentProductId);

            if (parent == null) {
                return AdminProductOperationResult.Failure("Parent not found");
            }

            product.IsDeleted = true;

            // If the last variant is removed, hide the parent product too.
            var hasOtherVariants = db.Products
                .IgnoreQueryFilters()
                .Any(x =>
                    x.ParentProductId == product.ParentProductId &&
                    x.Id != product.Id &&
                    !x.IsDeleted);

            if (!hasOtherVariants) {
                parent.IsDeleted = true;
            }

            db.SaveChanges();

            return AdminProductOperationResult.Success();
        }
    }
}
