using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class RoleController : Controller {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // ================= LIST =================
        public IActionResult Index(string keyword, int? page) {

            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.SearchQuery = keyword;

            var roles = _roleManager.Roles.AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(keyword)) {

                keyword = keyword.Trim();

                roles = roles.Where(x => x.Name.Contains(keyword));
            }

            var data = roles
                .OrderByDescending(x => x.Id)
                .Select(x => new RoleVM {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();

            int no = 1;

            foreach (var item in data) {

                item.No = no++;

                item.UserCount = _userManager.GetUsersInRoleAsync(item.Name)
                    .Result
                    .Count;
            }

            var result = data.ToPagedList(pageNumber, pageSize);

            ViewBag.Count = result.Count;

            return View(result);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult CreateRole() {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleCreateVM model) {

            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.Name)) {

                ModelState.AddModelError("Name", "Role name is required");

                return View(model);
            }

            var roleName = model.Name.Trim();

            bool exists = await _roleManager.RoleExistsAsync(roleName);

            if (exists) {

                ModelState.AddModelError("Name", "Role already exists");

                return View(model);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded) {

                foreach (var error in result.Errors) {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            return RedirectToAction("Index");
        }

        // ================= EDIT =================
        [HttpGet]
        public async Task<IActionResult> EditRole(string id) {

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return NotFound();

            var model = new RoleEditVM {
                Id = role.Id,
                Name = role.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(RoleEditVM model) {

            if (!ModelState.IsValid)
                return View(model);

            var role = await _roleManager.FindByIdAsync(model.Id);

            if (role == null)
                return NotFound();

            var roleName = model.Name.Trim();

            bool duplicate = _roleManager.Roles
                .Any(x => x.Id != model.Id && x.Name == roleName);

            if (duplicate) {

                ModelState.AddModelError("Name", "Role already exists");

                return View(model);
            }

            role.Name = roleName;
            role.NormalizedName = roleName.ToUpper();

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded) {

                foreach (var error in result.Errors) {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id) {

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null) {
                return Json(new {
                    success = false,
                    message = "Role not found"
                });
            }

            var users = await _userManager.GetUsersInRoleAsync(role.Name);

            if (users.Any()) {
                return Json(new {
                    success = false,
                    message = "Cannot delete this role because it currently in used."
                });
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded) {
                return Json(new {
                    success = false,
                    message = result.Errors.FirstOrDefault()?.Description ?? "Delete failed"
                });
            }

            return Json(new { success = true });
        }
    }
}