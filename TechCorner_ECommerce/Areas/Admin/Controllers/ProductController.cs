using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using System.Collections;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize]
    [Area("Admin")]
    public class ProductController : Controller {
        private readonly AppDbContext db;
        private readonly ISlugService _slugService;
        private readonly IWebHostEnvironment _webHost;

        public ProductController(AppDbContext context, ISlugService slugService, IWebHostEnvironment webHost) {
            db = context;
            _slugService = slugService;
            _webHost = webHost;
        }

        // ================= LOAD UI DATA =================
        private List<CategoryVM> LoadCategories() {
            return db.Categories
                .Select(c => new CategoryVM {
                    Id = c.CategoryId,
                    Name = c.Name
                }).ToList();
        }

        private List<SubCategoryVM> LoadSubCategories() {
            return db.SubCategories
                .Select(s => new SubCategoryVM {
                    Id = s.Id,
                    Name = s.Name,
                    CategoryId = s.CategoryId
                }).ToList();
        }

        private List<ProductAttributeVM> LoadAttributes() {
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
                }).ToList();
        }




        // ================= CREATE =================
        [HttpGet]
        public IActionResult AddProduct() {
            var model = new CreateProductVM {
                Categories = LoadCategories(),
                SubCategories = LoadSubCategories(),
                Attributes = LoadAttributes()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(CreateProductVM model) {
            

            if (!ModelState.IsValid) {
                model.Categories = LoadCategories();
                model.SubCategories = LoadSubCategories();
                model.Attributes = LoadAttributes();

                return View("AddProduct", model);
            }

            //// Check product duplicatetrong cùng subcategory
            var slug = _slugService.CreateSlug(model.Name.Trim());

            // Nếu slug đã tồn tại, thêm số vào cuối để tạo slug mới, chống trùng slug khi tạo
            int i = 1;
            string baseSlug = slug;

            while (db.ParentProducts.Any(x => x.Slug == slug)) {
                slug = $"{baseSlug}-{i}";
                i++;
            }

            bool exists = db.ParentProducts.Any(x => x.Slug == slug && x.SubCategoryId == model.SubCategoryId);

            if (exists) {
                ModelState.AddModelError("Name", "Product already exists in this subcategory");

                model.Categories = LoadCategories();
                model.SubCategories = LoadSubCategories();
                model.Attributes = LoadAttributes();

                return View("AddProduct", model);
            }

            //  Gom tất cả các thao tác DB vào một transaction để khi có lỗi sẽ rollback lại, tránh lưu dữ liệu mà bị thiếu
            using var transaction = await db.Database.BeginTransactionAsync();

            var publicId = CodeGenerator.Generate("PROD");

            while (db.ParentProducts.Any(x => x.PublicId == publicId)) {
                publicId = CodeGenerator.Generate("PROD");
            }

            try {
                var parent = new ParentProduct {
                    PublicId = publicId,
                    Name = model.Name,
                    Slug = _slugService.CreateSlug(model.Name),
                    Description = model.Description,
                    SubCategoryId = model.SubCategoryId
                };

                db.ParentProducts.Add(parent);
                await db.SaveChangesAsync();


                // Lưu Image
                if (model.Images != null && model.Images.Any()) {
                    bool isPrimary = true;
                    var uploadPath = Path.Combine(_webHost.WebRootPath, "images");

                    if (!Directory.Exists(uploadPath)) {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var file in model.Images) {
                        if (file == null || file.Length == 0)
                            continue;

                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

                        var path = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(path, FileMode.Create)) {
                            await file.CopyToAsync(stream);
                        }



                        db.ProductImages.Add(new ProductImage {
                            ParentProductId = parent.Id,
                            ImageUrl = "/images/" + fileName,
                            IsPrimary = isPrimary
                        });

                        isPrimary = false;
                    }
                }

                // Lưu các variant
                if (model.Variants != null) {
                    foreach (var v in model.Variants) {
                        var sku = CodeGenerator.Generate("SKU");

                        while (db.Products.Any(x => x.SkuCode == sku)) {
                            sku = CodeGenerator.Generate("SKU");
                        }

                        var product = new Product {
                            SkuCode = sku,
                            ParentProductId = parent.Id,
                            Price = v.Price,
                            StockQuantity = v.StockQuantity
                        };

                        db.Products.Add(product);
                        await db.SaveChangesAsync();

                        var pavs = v.AttributeValueIds.Select(attrId =>
                            new ProductAttributeValue {
                                ProductId = product.Id,
                                AttributeValueId = attrId
                            });

                        db.ProductAttributeValues.AddRange(pavs);
                    }
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("AddProduct");
            }
            catch (Exception ex) {
                await transaction.RollbackAsync();

                model.Categories = LoadCategories();
                model.SubCategories = LoadSubCategories();
                model.Attributes = LoadAttributes();

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

                .FirstOrDefault(x => x.PublicId == id );

            if (product == null)
                return NotFound();

            var model = new EditProductVM {

                ParentProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                SubCategoryId = product.SubCategoryId,

                Categories = LoadCategories(),
                SubCategories = LoadSubCategories(),
                Attributes = LoadAttributes(),

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

                    }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(EditProductVM model) {

            var parent = db.ParentProducts
                .Include(x => x.Products)
                .FirstOrDefault(x => x.Id == model.ParentProductId);

            if (parent == null)
                return NotFound();

            parent.Name = model.Name;
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

                foreach (var v in model.Variants) {
                    if (v.ProductId > 0) {
                        var product = db.Products.Find(v.ProductId);

                        if (product == null)
                            continue;

                        product.Price = v.Price;
                        product.StockQuantity = v.StockQuantity;
                    }
                    else {
                        if (v.AttributeValueIds == null || !v.AttributeValueIds.Any())
                            continue;

                        var newVariantKey = string.Join(",", v.AttributeValueIds.OrderBy(x => x));

                        if (existingVariantKeys.Contains(newVariantKey))
                            continue;

                        var newProduct = new Product {
                            ParentProductId = parent.Id,
                            Price = v.Price,
                            StockQuantity = v.StockQuantity
                        };

                        db.Products.Add(newProduct);
                        await db.SaveChangesAsync();

                        var pavs = v.AttributeValueIds.Select(attrId =>
                            new ProductAttributeValue {
                                ProductId = newProduct.Id,
                                AttributeValueId = attrId
                            });

                        db.ProductAttributeValues.AddRange(pavs);

                        existingVariantKeys.Add(newVariantKey);
                    }
                }
            }

            // upload new images
            if (model.NewImages != null && model.NewImages.Any()) {

                var uploadPath =
                    Path.Combine(_webHost.WebRootPath, "images");

                foreach (var file in model.NewImages) {

                    if (file == null || file.Length == 0)
                        continue;

                    var fileName =
                        Guid.NewGuid() + Path.GetExtension(file.FileName);

                    var path =
                        Path.Combine(uploadPath, fileName);

                    using var stream =
                        new FileStream(path, FileMode.Create);

                    await file.CopyToAsync(stream);

                    db.ProductImages.Add(new ProductImage {

                        ParentProductId = parent.Id,

                        ImageUrl = "/images/" + fileName
                    });
                }
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Index", "Inventory");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteImage(int id) {
            var image = db.ProductImages
                .FirstOrDefault(x => x.Id == id);

            if (image == null) {
                return Json(new {
                    success = false,
                    message = "Image not found"
                });
            }

            if (!string.IsNullOrEmpty(image.ImageUrl)) {
                var relativePath = image.ImageUrl.TrimStart('/');

                var fullPath = Path.Combine(
                    _webHost.WebRootPath,
                    relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (System.IO.File.Exists(fullPath)) {
                    System.IO.File.Delete(fullPath);
                }
            }

            db.ProductImages.Remove(image);

            db.SaveChanges();

            return Json(new {
                success = true
            });
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

            bool hasOtherVariants = db.Products
                .IgnoreQueryFilters()
                .Any(x =>
                    x.ParentProductId == product.ParentProductId &&
                    x.Id != product.Id &&
                    !x.IsDeleted
                );

            if (!hasOtherVariants) {
                parent.IsDeleted = true;
            }

            db.SaveChanges();

            return Json(new { success = true });
        }



    }
}