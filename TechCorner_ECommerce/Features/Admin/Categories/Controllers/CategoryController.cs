using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class CategoryController : Controller {
        private readonly IAdminCategoryService adminCategoryService;

        public CategoryController(IAdminCategoryService adminCategoryService) {
            this.adminCategoryService = adminCategoryService;
        }

        ///////////////////////// LIST /////////////////////////
        public IActionResult Index(string? search, int? page) {
            const int pageSize = 10;
            var pageNumber = page ?? 1;
            var result = adminCategoryService.GetCategories(search, pageNumber, pageSize);

            ViewBag.Search = result.Search;

            return View(result.Items);
        }

        ///////////////////////// CREATE /////////////////////////
        [HttpGet]
        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryCreateVM vm) {
            var result = adminCategoryService.CreateCategory(vm);

            if (!result.Succeeded) {
                ModelState.AddModelError(result.FieldName ?? "", result.Message);
                return View(vm);
            }

            return RedirectToAction("Index");
        }

        ///////////////////////// EDIT /////////////////////////
        [HttpGet]
        public IActionResult Edit(int id) {
            var category = adminCategoryService.GetCategoryEdit(id);

            return View(category);
        }

        ///////////////////////// DELETE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryEditVM vm) {
            var result = adminCategoryService.EditCategory(vm);

            if (result.NotFound) {
                return NotFound();
            }

            if (!result.Succeeded) {
                ModelState.AddModelError(result.FieldName ?? "", result.Message);
                return View(vm);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) {
            var result = adminCategoryService.DeleteCategory(id);

            if (!result.Succeeded) {
                return Json(new {
                    success = false,
                    message = result.Message
                });
            }

            return Json(new { success = true });
        }
    }
}
