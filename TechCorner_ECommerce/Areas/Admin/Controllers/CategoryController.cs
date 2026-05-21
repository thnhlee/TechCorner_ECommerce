using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Area("Admin")]
    [Authorize]
    public class CategoryController : Controller {
        private readonly AppDbContext db;

        public CategoryController(AppDbContext context) {
            db = context;
        }

        // ================= LIST =================
        public IActionResult Index(string search, int? page) {

            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = db.Categories
                .Include(x => x.SubCategories)
                    .ThenInclude(x => x.ParentProducts)
                        .ThenInclude(x => x.Products)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search)) {

                search = search.Trim();

                query = query.Where(x => x.Name.Contains(search));
            }

            var data = query.Select(x => new MenuCategoryVM {
                Id = x.CategoryId,
                Name = x.Name,
                CreatedAt = x.CreatedAt,
                ProductCount = x.SubCategories
                    .SelectMany(s => s.ParentProducts)
                    .SelectMany(p => p.Products)
                    .Count(),

                SubCategories = x.SubCategories.Select(s => new MenuSubCategoryVM {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToList()

            })
            .ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;

            return View(data);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryCreateVM vm) {

            // ================= VALIDATION =================
            if (string.IsNullOrWhiteSpace(vm.Name)) {
                ModelState.AddModelError("Name", "Category name is required");
            }
            else {
                bool exists = db.Categories.Any(x => x.Name.Trim().ToLower() == vm.Name.Trim().ToLower());

                if (exists) {
                    ModelState.AddModelError("Name", "Category already exists");
                }
            }

            if (!ModelState.IsValid) {
                return View(vm);
            }

            // ================= SAVE =================
            var category = new Category {
                Name = vm.Name,
                CreatedAt = DateTime.Now
            };

            db.Categories.Add(category);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================= EDIT =================
        [HttpGet]
        public IActionResult Edit(int id) {

            var category = db.Categories
                .Where(x => x.CategoryId == id)
                .Select(x => new CategoryEditVM {
                    CategoryId = x.CategoryId,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefault();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryEditVM vm) {

            // ================= VALIDATION (Check trong table đã tồn tại cate này chưa) =================
            if (string.IsNullOrWhiteSpace(vm.Name)) {
                ModelState.AddModelError("Name", "Category name is required");
            }
            else {
                bool exists = db.Categories.Any(x => x.CategoryId != vm.CategoryId && x.Name.Trim().ToLower() == vm.Name.Trim().ToLower());

                if (exists) {
                    ModelState.AddModelError("Name", "Category already exists");
                }
            }
            if (!ModelState.IsValid) {
                return View(vm);
            }
            var category = db.Categories.Find(vm.CategoryId);

            category.Name = vm.Name;

            category.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) {

            var cate = db.Categories
                .Include(x => x.SubCategories)
                .ThenInclude(x => x.ParentProducts)
                .FirstOrDefault(x => x.CategoryId == id);

            if (cate == null) {
                return Json(new {
                    success = false,
                    message = "Category not found"
                });
            }

            bool hasProducts = cate.SubCategories
                .Any(x => x.ParentProducts.Any());

            if (hasProducts) {
                return Json(new {
                    success = false,
                    message = "This category cannot be deleted because it is already used."
                });
            }

            db.Categories.Remove(cate);

            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
