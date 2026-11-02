using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class RoleController : Controller {
        private readonly IAdminRoleService adminRoleService;

        public RoleController(IAdminRoleService adminRoleService) {
            this.adminRoleService = adminRoleService;
        }

        ///////////////////////// LIST /////////////////////////
        public async Task<IActionResult> Index(string? keyword, int? page) {
            const int pageSize = 10;
            var pageNumber = page ?? 1;

            ViewBag.SearchQuery = keyword;

            var result = await adminRoleService.GetRolesAsync(keyword, pageNumber, pageSize);

            ViewBag.Count = result.Count;

            return View(result.Items);
        }

        ///////////////////////// CREATE /////////////////////////
        [HttpGet]
        public IActionResult CreateRole() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleCreateVM model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var result = await adminRoleService.CreateRoleAsync(model);

            if (!result.Succeeded) {
                AddRoleErrors(result);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        ///////////////////////// EDIT /////////////////////////
        [HttpGet]
        public async Task<IActionResult> EditRole(string id) {
            var model = await adminRoleService.GetEditRoleAsync(id);

            if (model == null) {
                return NotFound();
            }

            return View(model);
        }

        ///////////////////////// DELETE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(RoleEditVM model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var result = await adminRoleService.EditRoleAsync(model);

            if (result.NotFound) {
                return NotFound();
            }

            if (!result.Succeeded) {
                AddRoleErrors(result);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id) {
            var result = await adminRoleService.DeleteRoleAsync(id);

            if (result.NotFound) {
                return JsonError(result.Message);
            }

            if (!result.Succeeded) {
                return JsonError(result.Message);
            }

            return Json(new { success = true });
        }

        private void AddRoleErrors(AdminRoleOperationResult result) {
            if (!string.IsNullOrWhiteSpace(result.Message)) {
                ModelState.AddModelError(result.FieldName ?? "", result.Message);
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error.Description);
            }
        }

        private JsonResult JsonError(string message) {
            return Json(new {
                success = false,
                message
            });
        }
    }
}
