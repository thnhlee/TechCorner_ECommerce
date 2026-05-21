using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Area("Admin")]
    public class SubCategoryController : Controller {
        private readonly AppDbContext db;

        public SubCategoryController(AppDbContext context) {
            db = context;
        }

        // ================= LIST =================
        public IActionResult Index(int? categoryId, string search, int? page) {

            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = db.SubCategories
                .Include(x => x.Category)
                .Include(x => x.ParentProducts)
                .AsQueryable();

            // FILTER CATEGORY
            if (categoryId.HasValue)
                query = query.Where(x => x.CategoryId == categoryId);

            // SEARCH
            if (!string.IsNullOrEmpty(search)) {

                search = search.Trim();

                query = query.Where(x => x.Name.Contains(search));
            }
                

            var data = query.Select(x => new MenuSubCategoryVM {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                //CreatedAt = x.CreatedAt,
                Quantity = x.ParentProducts
                    .SelectMany(p => p.Products)
                    .Count(),
            }).ToPagedList(pageNumber, pageSize);

            ViewBag.Categories = db.Categories.ToList();

            ViewBag.categoryId = categoryId;
            ViewBag.Search = search;

            return View(data);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create() {
            var model = new SubCategoryCreateVM {
                Categories = db.Categories
                    .Select(x => new CategoryVM {
                        Id = x.CategoryId,
                        Name = x.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SubCategoryCreateVM vm) {

            // ================= VALIDATION =================
            if (string.IsNullOrWhiteSpace(vm.Name)) {
                ModelState.AddModelError("Name",
                    "SubCategory name is required");
            }

            if (vm.CategoryId == 0) {
                ModelState.AddModelError("CategoryId",
                    "Please select category");
            }
            else {
                // duplicate
                bool exists = db.SubCategories.Any(x => x.Name.Trim() == vm.Name.Trim() && x.CategoryId == vm.CategoryId);

                if (exists) {
                    ModelState.AddModelError("Name", "SubCategory already exists in this category");
                }
            }




            if (!ModelState.IsValid) {
                vm.Categories = db.Categories
                    .Select(x => new CategoryVM {
                        Id = x.CategoryId,
                        Name = x.Name
                    })
                    .ToList();

                return View(vm);
            }


            var subCategory = new SubCategory {
                Name = vm.Name,
                CategoryId = vm.CategoryId
            };

            db.SubCategories.Add(subCategory);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================= EDIT =================
        [HttpGet]
        public IActionResult Edit(int id) {
            var model = db.SubCategories
                .Where(x => x.Id == id)
                .Select(x => new SubCategoryEditVM {
                    Id = x.Id,
                    Name = x.Name,
                    CategoryId = x.CategoryId
                })
                .FirstOrDefault();

            if (model == null) {
                return NotFound();
            }

            ViewBag.Categories = db.Categories.ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SubCategoryEditVM vm) {

            // ================= VALIDATION (Check trong cate này đã tồn tại subcate này chưa) =================
            if (string.IsNullOrWhiteSpace(vm.Name)) {
                ModelState.AddModelError("Name",
                    "SubCategory name is required");
            }

            if (vm.CategoryId == 0) {
                ModelState.AddModelError("CategoryId",
                    "Please select category");
            }
            else {
                // duplicate
                bool exists = db.SubCategories.Any(x => x.Id != vm.Id && x.Name.Trim() == vm.Name.Trim() && x.CategoryId == vm.CategoryId);

                if (exists) {
                    ModelState.AddModelError("Name", "SubCategory already exists in this category");
                }
            }


            if (!ModelState.IsValid) {
                ViewBag.Categories = db.Categories.ToList();
                return View(vm);
            }

            var subCate = db.SubCategories.Find(vm.Id);

            if (subCate == null) {
                return NotFound();
            }

            subCate.Name = vm.Name;
            subCate.CategoryId = vm.CategoryId;

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) {
            var sub = db.SubCategories
                .Include(x => x.ParentProducts)
                .FirstOrDefault(x => x.Id == id);

            if (sub == null)
                return Json(new { success = false, message = "Not found" });

            if (sub.ParentProducts.Any()) {
                return Json(new {
                    success = false,
                    message = "This subcategory cannot be deleted because it is already used."
                });
            }

            db.SubCategories.Remove(sub);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
