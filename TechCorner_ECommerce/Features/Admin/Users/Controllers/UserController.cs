using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class UserController : Controller {
        private readonly IAdminUserService adminUserService;

        public UserController(IAdminUserService adminUserService) {
            this.adminUserService = adminUserService;
        }

        ///////////////////////// LIST /////////////////////////
        public async Task<IActionResult> Index(string? keyword, int? page) {
            const int pageSize = 10;
            var pageNumber = page ?? 1;

            ViewBag.SearchQuery = keyword;

            var result = await adminUserService.GetUsersAsync(keyword, pageNumber, pageSize);

            ViewBag.Count = result.Count;

            return View(result.Items);
        }

        ///////////////////////// EDIT /////////////////////////
        [HttpGet]
        public async Task<IActionResult> Edit(string id) {
            var guard = await adminUserService.CanEditUserAsync(User, id);

            if (guard.Forbidden) {
                TempData["Error"] = guard.Message;
                return RedirectToAction("Index");
            }

            var model = await adminUserService.GetEditUserAsync(id);

            if (model == null) {
                return NotFound();
            }

            return View(model);
        }

        ///////////////////////// DELETE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVM model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var result = await adminUserService.EditUserAsync(model);

            if (result.NotFound) {
                return NotFound();
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error.Description);
            }

            if (result.Errors.Any()) {
                model.Roles = adminUserService.GetRoleNames();
                return View(model);
            }

            TempData["Success"] = result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id) {
            var result = await adminUserService.DeleteUserAsync(User, id);

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
