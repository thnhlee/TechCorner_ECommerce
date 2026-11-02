using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Controllers {
    [Authorize]
    public class ProfileController : Controller {
        private readonly IProfileService profileService;

        public ProfileController(IProfileService profileService) {
            this.profileService = profileService;
        }

        ///////////////////////// PROFILE DETAIL /////////////////////////
        [HttpGet]
        public async Task<IActionResult> Index() {
            var result = await profileService.GetProfileAsync(User);

            if (result.RequiresLogin) {
                return RedirectToAction("Login", "Account");
            }

            return View(result.Value);
        }

        ///////////////////////// UPDATE PROFILE /////////////////////////
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileVM model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var result = await profileService.UpdateProfileAsync(User, model);

            if (result.RequiresLogin) {
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error.Description);
            }

            if (result.Errors.Any()) {
                return View(model);
            }

            TempData["Success"] = "Profile updated successfully";

            return RedirectToAction("Index");
        }
    }
}
