using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Controllers {
    [Authorize]
    public class ProfileController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext db;

        public ProfileController(UserManager<ApplicationUser> userManager, AppDbContext context) {
            _userManager = userManager;
            db = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var address = db.Addresses
                .FirstOrDefault(x => x.UserId == user.Id);

            var model = new ProfileVM {
                Email = user.Email,

                UserName = string.IsNullOrWhiteSpace(user.UserName)
                    ? user.Email
                    : user.UserName,

                

                ReceiverName = address?.ReceiverName,
                Phone = address?.Phone,
                FullAddress = address?.FullAddress
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileVM model) {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            user.UserName = string.IsNullOrWhiteSpace(model.UserName)
                ? user.Email
                : model.UserName.Trim();



            var updateUserResult = await _userManager.UpdateAsync(user);

            if (!updateUserResult.Succeeded) {
                foreach (var error in updateUserResult.Errors) {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            var address = db.Addresses
                .FirstOrDefault(x => x.UserId == user.Id);

            if (address == null) {
                address = new Address {
                    UserId = user.Id,
                    ReceiverName = model.ReceiverName?.Trim(),
                    Phone = model.Phone?.Trim(),
                    FullAddress = model.FullAddress?.Trim(),
                    CreatedAt = DateTime.Now
                };

                db.Addresses.Add(address);
            }
            else {
                address.ReceiverName = model.ReceiverName?.Trim();
                address.Phone = model.Phone?.Trim();
                address.FullAddress = model.FullAddress?.Trim();
                address.UpdatedAt = DateTime.Now;
            }

            await db.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully";

            return RedirectToAction("Index");
        }
    }
}