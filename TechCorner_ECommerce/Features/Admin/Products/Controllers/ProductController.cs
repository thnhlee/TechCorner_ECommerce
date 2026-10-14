using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ProductController : Controller {
        private readonly ICatalogLookupService catalogLookup;
        private readonly IAdminProductService adminProductService;

        public ProductController(
            ICatalogLookupService catalogLookup,
            IAdminProductService adminProductService) {
            this.catalogLookup = catalogLookup;
            this.adminProductService = adminProductService;
        }

        ///////////////////////// CREATE /////////////////////////
        [HttpGet]
        public IActionResult AddProduct() {
            var model = new CreateProductVM();
            catalogLookup.PopulateProductLookups(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(CreateProductVM model) {
            if (!ModelState.IsValid) {
                catalogLookup.PopulateProductLookups(model);
                return View("AddProduct", model);
            }

            var result = await adminProductService.AddProductAsync(model);

            if (!result.Succeeded) {
                ModelState.AddModelError(result.FieldName ?? "", result.Message);
                catalogLookup.PopulateProductLookups(model);
                return View("AddProduct", model);
            }

            return RedirectToAction("AddProduct");
        }

        ///////////////////////// EDIT /////////////////////////
        [HttpGet]
        public IActionResult EditProduct(string id) {
            var model = adminProductService.GetEditProduct(id);

            if (model == null) {
                return NotFound();
            }

            return View(model);
        }

        ///////////////////////// DELETE IMAGE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(EditProductVM model) {
            var result = await adminProductService.EditProductAsync(model);

            if (result.NotFound) {
                return NotFound();
            }

            return RedirectToAction("Index", "Inventory");
        }

        ///////////////////////// DELETE VARIANT /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteImage(int id) {
            var result = adminProductService.DeleteImage(id);

            if (!result.Succeeded) {
                return JsonError(result.Message);
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVariant(int id) {
            var result = adminProductService.DeleteVariant(id);

            if (!result.Succeeded) {
                return JsonError(result.Message);
            }

            return Json(new { success = true });
        }

        private JsonResult JsonError(string message) {
            return Json(new {
                success = false,
                message
            });
        }
    }
}
