using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class SubCategoryController : Controller {
        private readonly IAdminCategoryService adminCategoryService;

        public SubCategoryController(IAdminCategoryService adminCategoryService) {
            this.adminCategoryService = adminCategoryService;
        }

        ///////////////////////// LIST /////////////////////////
        public IActionResult Index(int? categoryId, string? search, int? page) {
            const int pageSize = 10;
            var pageNumber = page ?? 1;
            var result = adminCategoryService.GetSubCategories(categoryId, search, pageNumber, pageSize);

            ViewBag.Categories = result.Categories;
            ViewBag.categoryId = result.CategoryId;
            ViewBag.Search = result.Search;

            return View(result.Items);
        }

        ///////////////////////// CREATE /////////////////////////
        [HttpGet]
        public IActionResult Create() {
            return View(adminCategoryService.GetSubCategoryCreateModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SubCategoryCreateVM vm) {
            var result = adminCategoryService.CreateSubCategory(vm);

            if (!result.Succeeded) {
                ModelState.AddModelError(result.FieldName ?? "", result.Message);
                vm.Categories = adminCategoryService.GetSubCategoryCreateModel().Categories;
                return View(vm);
            }

            return RedirectToAction("Index");
        }

        ///////////////////////// EDIT /////////////////////////
        [HttpGet]
        public IActionResult Edit(int id) {
            var model = adminCategoryService.GetSubCategoryEdit(id);

            if (model == null) {
                return NotFound();
            }

            ViewBag.Categories = adminCategoryService.GetCategoryEntities();

            return View(model);
        }

        ///////////////////////// DELETE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SubCategoryEditVM vm) {
            var result = adminCategoryService.EditSubCategory(vm);

            if (result.NotFound) {
                return NotFound();
            }

            if (!result.Succeeded) {
                ModelState.AddModelError(result.FieldName ?? "", result.Message);
                ViewBag.Categories = adminCategoryService.GetCategoryEntities();
                return View(vm);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) {
            var result = adminCategoryService.DeleteSubCategory(id);

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
