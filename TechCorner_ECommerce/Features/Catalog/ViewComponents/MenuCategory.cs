using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.ViewComponents {
    public class MenuCategory : ViewComponent {
        private readonly AppDbContext db;

        public MenuCategory(AppDbContext context) => db = context; 

        public IViewComponentResult Invoke() {
            var data = db.Categories
                .Include(c => c.SubCategories)
                .ThenInclude(sc => sc.ParentProducts)
                .ThenInclude(pp => pp.Products)
                .Select(c => new MenuCategoryVM {
                Id = c.CategoryId,
                Name = c.Name,
                
                SubCategories = c.SubCategories.Select(sc => new MenuSubCategoryVM {
                    Id = sc.Id,
                    Name = sc.Name,

                    //// Tổng tất cả sp ở mọi parent product của sub category 
                    //Quantity = sc.ParentProducts.SelectMany(pp => pp.Products).Count()
                    //// Tổng parent product của sub category 
                    Quantity = sc.ParentProducts.Count()
                }).ToList()
            });
            return View(data);
        }

    }
}
