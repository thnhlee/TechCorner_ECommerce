using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;
using TechCorner_ECommerce.Helpers;

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



        // ================= GET =================
        [HttpGet]
        public IActionResult AddProduct() {
            var model = new CreateProductVM {
                Categories = LoadCategories(),
                SubCategories = LoadSubCategories(),
                Attributes = LoadAttributes()
            };

            return View(model);
        }

        // ================= POST =================
        [HttpPost]
        public async Task<IActionResult> AddProduct(CreateProductVM model) {
            
            if (string.IsNullOrEmpty(model.Name)) {
                ModelState.AddModelError("Name", "Name is required");
            }
            if (!ModelState.IsValid) {
                model.Categories = LoadCategories();
                model.SubCategories = LoadSubCategories();
                model.Attributes = LoadAttributes();

                return View("AddProduct", model);
            }

            var seen = new HashSet<string>();

            if (model.Variants != null) {
                foreach (var v in model.Variants) {
                    if (v.AttributeValueIds == null || !v.AttributeValueIds.Any()) {
                        ModelState.AddModelError("", "Variant must have attributes.");

                        model.Categories = LoadCategories();
                        model.SubCategories = LoadSubCategories();
                        model.Attributes = LoadAttributes();

                        return View("AddProduct", model);
                    }

                    var key = string.Join("-", v.AttributeValueIds.OrderBy(x => x));

                    if (!seen.Add(key)) {
                        ModelState.AddModelError("", "Duplicate variant detected.");

                        model.Categories = LoadCategories();
                        model.SubCategories = LoadSubCategories();
                        model.Attributes = LoadAttributes();

                        return View("AddProduct", model);
                    }
                }
            }

            //  Gom tất cả các thao tác DB vào một transaction để khi có lỗi sẽ rollback lại, tránh lưu dữ liệu mà bị thiếu
            using var transaction = await db.Database.BeginTransactionAsync();

            try {
                var parent = new ParentProduct {
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

                        var product = new Product {
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
                //Console.WriteLine("LỖI: " + ex.ToString());
                //ModelState.AddModelError("", ex.Message);
                model.Categories = LoadCategories();
                model.SubCategories = LoadSubCategories();
                model.Attributes = LoadAttributes();

                ModelState.AddModelError("", ex.Message);

                return View("AddProduct", model);
            }
        }
    }
}