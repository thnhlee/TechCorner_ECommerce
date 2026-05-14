using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Helpers;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize]
    [Area("Admin")]
    public class InventoryController : Controller {
        private readonly AppDbContext db;

        public InventoryController(AppDbContext context) {
            db = context;
        }
        // ================= INVENTORY =================
        public IActionResult Index(int? cate, string keyword) {

            ////=========== Show ra Product Parent ===========
            //var products = db.ParentProducts
            //    .Include(p => p.SubCategory)
            //        .ThenInclude(sc => sc.Category)
            //    .Include(p => p.Images)
            //    .Include(p => p.Products)
            //    .AsQueryable();

            //// filter by subcategory
            //if (cate.HasValue) {
            //    products = products.Where(p => p.SubCategoryId == cate.Value);
            //}

            //// search
            //if (!string.IsNullOrEmpty(keyword)) {
            //    //// Search không phân biệt chữ hoa chữ thường
            //    //products = products.Where(p => p.Name.ToLower().Contains(keyword.ToLower()));

            //    //// Search phân biệt chữ hoa chữ thường
            //    //products = products.Where(p => p.Name.Contains(keyword));

            //    //// Search sử dụng EF.Functions.Like để hỗ trợ wildcard và không phân biệt chữ hoa chữ thường
            //    //products = products.Where(p => EF.Functions.Like(p.Name, $"%{keyword}%"));

            //    //// ép kiểu collation để search không phân biệt chữ hoa chữ thường và dấu tiếng Việt
            //    products = products.Where(p => EF.Functions.Like(EF.Functions.Collate(p.Name, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%"
            //        )
            //    );

            //    ViewBag.SearchQuery = keyword;

            //}

            //var result = products.Select(p => new ProductVM {
            //    Id = p.Id,
            //    Name = p.Name,
            //    Description = p.Description,

            //    // lấy giá nhỏ nhất từ Product(Variant)
            //    Price = p.Products.Min(v => (decimal?)v.Price) ?? 0,

            //    // lấy ảnh đại diện
            //    ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl ?? p.Images.FirstOrDefault().ImageUrl ?? "",

            //    Stock = p.Products.Sum(v => v.StockQuantity),
            //    CategoryName = p.SubCategory.Category.Name,
            //    SubCategoryName = p.SubCategory.Name
            //})
            //.ToList();
            //ViewBag.Count = result.Count;
            //return View(result);


            //=========== Show ra toàn bộ Product variant ===========
            var products = db.Products
                    .Include(x => x.ParentProduct)
                    .ThenInclude(x => x.SubCategory)
                        .ThenInclude(x => x.Category)

                    .Include(x => x.ParentProduct)
                        .ThenInclude(x => x.Images)

                    .Include(x => x.ProductAttributeValues)
                        .ThenInclude(x => x.AttributeValue)

                    .AsQueryable();

            // FILTER SUBCATEGORY
            if (cate.HasValue) {
                products = products.Where(x => x.ParentProduct.SubCategoryId == cate.Value);
            }

            // SEARCH
            if (!string.IsNullOrWhiteSpace(keyword)) {
                keyword = keyword.Trim();

                products = products.Where(x =>
                    EF.Functions.Like(
                        EF.Functions.Collate(
                            x.ParentProduct.Name,
                            "SQL_Latin1_General_CP1_CI_AI"
                        ),
                        $"%{keyword}%"
                    ));

                
                ViewBag.SearchQuery = keyword;
            }

            var result = products.Select(x => new ProductVM {

                Id = x.Id,
                ParentProductId = x.ParentProductId,

                Name = x.ParentProduct.Name,
                Description = x.ParentProduct.Description,
                Price = x.Price,

                Stock = x.StockQuantity,

                CategoryName = x.ParentProduct.SubCategory.Category.Name,

                SubCategoryName = x.ParentProduct.SubCategory.Name,

                ImageUrl = x.ParentProduct.Images
                                .Where(i => i.IsPrimary)
                                .Select(i => i.ImageUrl)
                                .FirstOrDefault()??
                            x.ParentProduct.Images
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()??"",

                Attributes = x.ProductAttributeValues
                    .Select(v => new AttributeVM {

                        Name = v.AttributeValue.ProductAttribute.Name,

                        Value = v.AttributeValue.Value

                    })
                    .ToList()
            })
            .ToList();

            ViewBag.SubCategories = db.SubCategories.ToList();
            ViewBag.Count = result.Count;

            return View(result);
        }
    }
}
