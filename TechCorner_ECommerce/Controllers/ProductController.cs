using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Controllers {
    public class ProductController : Controller {
        private readonly AppDbContext db;

        public ProductController(AppDbContext context) {
            db = context;
        }

        public IActionResult Index(int? cate, string keyword) {
            var products = db.ParentProducts
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Images)
                .Include(p => p.Products)
                .AsQueryable();

            // filter by subcategory
            if (cate.HasValue) {
                products = products.Where(p => p.SubCategoryId == cate.Value);
            }

            // search
            if (!string.IsNullOrEmpty(keyword)) {
                //// Search không phân biệt chữ hoa chữ thường
                //products = products.Where(p => p.Name.ToLower().Contains(keyword.ToLower()));

                //// Search phân biệt chữ hoa chữ thường
                //products = products.Where(p => p.Name.Contains(keyword));

                //// Search sử dụng EF.Functions.Like để hỗ trợ wildcard và không phân biệt chữ hoa chữ thường
                //products = products.Where(p => EF.Functions.Like(p.Name, $"%{keyword}%"));

                //// ép kiểu collation để search không phân biệt chữ hoa chữ thường và dấu tiếng Việt
                products = products.Where(p => EF.Functions.Like(EF.Functions.Collate(p.Name, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%"
                    )
                );

                ViewBag.SearchQuery = keyword;

            }

            var result = products.Select(p => new ProductVM {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,

                // lấy giá nhỏ nhất từ Product(Variant)
                Price = p.Products.Min(v => (decimal?)v.Price) ?? 0,

                // lấy ảnh đại diện
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl ?? p.Images.FirstOrDefault().ImageUrl ?? "",

                CategoryName = p.SubCategory.Category.Name
            })
            .ToList();
            ViewBag.Count = result.Count;
            return View(result);
        }

        public IActionResult Detail(int id) {
            var result = db.ParentProducts
                .Where(p => p.Id == id)
                .Select(p => new ProductVM {
                    Id = p.Id,
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

                    Stock = p.Products.Sum(v => v.StockQuantity),

                    // variants list
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

            if (result == null) {
                return Redirect("/404"); 
            }

            return View(result);
        }

        [HttpGet]
        public IActionResult GetVariants(int productId) {
            var product = db.ParentProducts
                .Include(p => p.Images)
                .Include(p => p.Products)
                    .ThenInclude(sku => sku.ProductAttributeValues)
                        .ThenInclude(pav => pav.AttributeValue)
                            .ThenInclude(av => av.ProductAttribute)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null)
                return NotFound();

            var result = new {
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

            return Json(result);
        }
    }
}