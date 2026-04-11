using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.ViewComponents {
    public class MenuCategory : ViewComponent {
        private readonly AppDbContext db;

        public MenuCategory(AppDbContext context) => db = context;

        public IViewComponentResult Invoke() {
            var data = db.Categories.Select(lo => new MenuCategoryVM{
                Id = lo.Id,
                Name = lo.Name,

                SubCategories = lo.SubCategories.Select(slo => new MenuSubCategoryVM{
                    Id = slo.Id,
                    Name = slo.Name,
                    Quantity = slo.Products.Count()
                }).ToList()
            });
            return View(data);
        }

    }
}
