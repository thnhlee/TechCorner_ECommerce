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
            var products = db.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
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
                products = products.Where(p => EF.Functions.Like( EF.Functions.Collate(p.Name, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%"
                    )
                );

                ViewBag.SearchQuery = keyword;
            }

            var result = products.Select(p => new ProductVM {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,

                // lấy giá nhỏ nhất từ ProductVariant
                Price = p.Variants.Min(v => (decimal?)v.Price) ?? 0,

                // lấy ảnh đại diện
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl ?? p.Images.FirstOrDefault().ImageUrl ?? "",

                CategoryName = p.SubCategory.Category.Name
            });

            return View(result);
        }

        public IActionResult Detail(int id) {
            var product = db.Products
                .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .ThenInclude(v => v.VariantAttributeValues)
                .ThenInclude(vav => vav.AttributeValue)
                .ThenInclude(av => av.Attribute)
                .SingleOrDefault(p => p.Id == id);

            if (product == null) {
                return Redirect("/404");
            }

            var result = new ProductVM {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,

                Price = product.Variants.Min(v => (decimal?)v.Price) ?? 0,

                ImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl
                           ?? product.Images.FirstOrDefault().ImageUrl
                           ?? "",

                CategoryName = product.SubCategory.Category.Name,
                Stock = product.Variants.Sum(v => v.StockQuantity),

                Variants = product.Variants.Select(v => new VariantVM {
                    Id = v.Id,
                    Price = v.Price,
                    Stock = v.StockQuantity,
                    Attributes = v.VariantAttributeValues.Select(a => new AttributeVM {
                        Name = a.AttributeValue.Attribute.Name,
                        Value = a.AttributeValue.Value
                    }).ToList()
                }).ToList()
            };

            return View(result);
        }

        [HttpGet]
        public IActionResult GetVariants(int productId) {
            var product = db.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .ThenInclude(v => v.VariantAttributeValues)
                .ThenInclude(vav => vav.AttributeValue)
                .ThenInclude(av => av.Attribute)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null) return NotFound();

            var result = new {
                product.Id,
                product.Name,
                Image = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,

                Variants = product.Variants.Select(v => new
                {
                    v.Id,
                    v.Price,
                    v.StockQuantity,
                    Attributes = v.VariantAttributeValues.Select(a => new
                    {
                        Name = a.AttributeValue.Attribute.Name,
                        Value = a.AttributeValue.Value
                    })
                })
            };

            return Json(result);
        }
    }
}