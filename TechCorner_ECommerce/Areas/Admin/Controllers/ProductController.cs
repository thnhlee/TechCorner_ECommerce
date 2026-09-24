using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ProductController : Controller {
        private readonly AppDbContext db;
        private readonly ISlugService _slugService;
        private readonly ICatalogLookupService _catalogLookup;
        private readonly IProductImageService _productImageService;
        private readonly IUniqueCodeService _uniqueCodeService;

        public ProductController(
            AppDbContext context,
            ISlugService slugService,
            ICatalogLookupService catalogLookup,
            IProductImageService productImageService,
            IUniqueCodeService uniqueCodeService) {
            db = context;
            _slugService = slugService;
            _catalogLookup = catalogLookup;
            _productImageService = productImageService;
            _uniqueCodeService = uniqueCodeService;
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult AddProduct() {
            var model = new CreateProductVM();
            _catalogLookup.PopulateProductLookups(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(CreateProductVM model) {
            if (!ModelState.IsValid) {
                _catalogLookup.PopulateProductLookups(model);
                return View("AddProduct", model);
            }

            var normalizedName = model.Name.Trim().ToLower();
            var exists = db.ParentProducts.Any(x =>
                x.SubCategoryId == model.SubCategoryId &&
                x.Name.Trim().ToLower() == normalizedName);

            if (exists) {
                ModelState.AddModelError("Name", "Product already exists in this subcategory");
                _catalogLookup.PopulateProductLookups(model);
                return View("AddProduct", model);
            }

            //  Gom tất cả các thao tác DB vào một transaction để khi có lỗi sẽ rollback lại, tránh lưu dữ liệu mà bị thiếu
            await using var transaction = await db.Database.BeginTransactionAsync();

            try {
                var parent = new ParentProduct {
                    PublicId = _uniqueCodeService.CreateParentProductPublicId(),
                    Name = model.Name.Trim(),
                    Slug = _slugService.CreateSlug(model.Name),
                    Description = model.Description,
                    SubCategoryId = model.SubCategoryId
                };

                db.ParentProducts.Add(parent);
                await db.SaveChangesAsync();

                await _productImageService.AddImagesAsync(parent.Id, model.Images, firstImageIsPrimary: true);

                // Lưu các variant
                if (model.Variants != null) {
                    foreach (var variant in model.Variants) {
                        if (variant.AttributeValueIds == null || !variant.AttributeValueIds.Any()) {
                            continue;
                        }

                        var product = new Product {
                            SkuCode = _uniqueCodeService.CreateSkuCode(),
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

                return RedirectToAction("AddProduct");
            }
            catch (Exception ex) {
                await transaction.RollbackAsync();

                _catalogLookup.PopulateProductLookups(model);
                ModelState.AddModelError("", ex.Message);

                return View("AddProduct", model);
            }
        }

        // ================= EDIT =================
        [HttpGet]
        public IActionResult EditProduct(string id) {
            var product = db.ParentProducts
                .Include(x => x.SubCategory)
                .ThenInclude(x => x.Category)
                .Include(x => x.Images)
                .Include(x => x.Products)
                .ThenInclude(x => x.ProductAttributeValues)
                .ThenInclude(x => x.AttributeValue)
                .FirstOrDefault(x => x.PublicId == id);

            if (product == null) {
                return NotFound();
            }

            var model = new EditProductVM {
                ParentProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                SubCategoryId = product.SubCategoryId,
                Categories = _catalogLookup.GetCategories(),
                SubCategories = _catalogLookup.GetSubCategories(),
                Attributes = _catalogLookup.GetAttributes(),
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

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(EditProductVM model) {
            var parent = db.ParentProducts
                .Include(x => x.Products)
                .FirstOrDefault(x => x.Id == model.ParentProductId);

            if (parent == null) {
                return NotFound();
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

                        if (existingVariantKeys.Contains(newVariantKey)) {
                            continue;
                        }

                        var newProduct = new Product {
                            ParentProductId = parent.Id,
                            SkuCode = _uniqueCodeService.CreateSkuCode(),
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

            await _productImageService.AddImagesAsync(parent.Id, model.NewImages, firstImageIsPrimary: false);

            await db.SaveChangesAsync();

            return RedirectToAction("Index", "Inventory");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteImage(int id) {
            var image = db.ProductImages.FirstOrDefault(x => x.Id == id);

            if (image == null) {
                return Json(new {
                    success = false,
                    message = "Image not found"
                });
            }

            _productImageService.DeleteImageFile(image);
            db.ProductImages.Remove(image);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVariant(int id) {
            var product = db.Products
                .IgnoreQueryFilters()
                .FirstOrDefault(x => x.Id == id);

            if (product == null) {
                return Json(new {
                    success = false,
                    message = "Variant not found"
                });
            }

            var parent = db.ParentProducts
                .IgnoreQueryFilters()
                .FirstOrDefault(x => x.Id == product.ParentProductId);

            if (parent == null) {
                return Json(new {
                    success = false,
                    message = "Parent not found"
                });
            }

            // soft delete variant
            product.IsDeleted = true;

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

            return Json(new { success = true });
        }
    }
}
