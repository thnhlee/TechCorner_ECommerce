using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Controllers {
    public class ProductController : Controller {
        private readonly AppDbContext db;

        public ProductController(AppDbContext context) {
            db = context;
        }
        public IActionResult Index(int? cate) {
            var products = db.Products.AsQueryable();
            if (cate.HasValue) {
                products = products.Where(p => p.SubCategoryId == cate.Value);
            }
            var result = products.Select(p => new ProductVM{
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
