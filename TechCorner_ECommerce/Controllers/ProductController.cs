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
            var products = db.Products.AsQueryable();
            if (cate.HasValue) {
                products = products.Where(p => p.SubCategoryId == cate.Value);
            }

            //search theo tên sản phẩm
            if (!string.IsNullOrEmpty(keyword)) {
                //// Search không phân biệt chữ hoa chữ thường
                //products = products.Where(p => p.Name.ToLower().Contains(keyword.ToLower()));

                //// Search phân biệt chữ hoa chữ thường
                //products = products.Where(p => p.Name.Contains(keyword));

                //// Search sử dụng EF.Functions.Like để hỗ trợ wildcard và không phân biệt chữ hoa chữ thường
                //products = products.Where(p => EF.Functions.Like(p.Name, $"%{keyword}%"));

                //// ép kiểu collation để search không phân biệt chữ hoa chữ thường và dấu tiếng Việt
                products = products.Where(p => EF.Functions.Like(EF.Functions.Collate(p.Name, "SQL_Latin1_General_CP1_CI_AI"), $"%{keyword}%"));
            }


            var result = products.Select(p => new ProductVM {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                price = p.Price,
                ImageUrl = p.ImageUrl ?? "",
                CategoryName = p.Category.Name
            });
            return View(result);
        }
    }
}
