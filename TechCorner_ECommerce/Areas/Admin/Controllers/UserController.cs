using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Area("Admin")]
    [Authorize]
    public class UserController : Controller {
        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(AppDbContext context, UserManager<ApplicationUser> userManager) {
            db = context;
            _userManager = userManager;
        }

        // ================= LIST =================
        public IActionResult Index(string keyword, int? page) {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.SearchQuery = keyword;

            var users = db.Users
                .GroupJoin(
                    db.Addresses,
                    user => user.Id,
                    address => address.UserId,
                    (user, addresses) => new {
                        User = user,
                        Address = addresses.FirstOrDefault()
                    }
                )
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword)) {
                keyword = keyword.Trim();

                users = users.Where(x =>
                    x.User.UserName.Contains(keyword) ||
                    x.User.Email.Contains(keyword) ||
                    x.Address.Phone.Contains(keyword) ||
                    x.Address.FullAddress.Contains(keyword)
                );
            }

            var data = users
                .OrderByDescending(x => x.User.Id)
                .Select(x => new UserListVM {
                    Id = x.User.Id,
                    UserName = x.User.UserName,
                    Email = x.User.Email,
                    Phone = x.Address != null ? x.Address.Phone : "",
                    Address = x.Address != null ? x.Address.FullAddress : ""
                })
                .ToList();

            int no = 1;
            foreach (var item in data) {
                item.No = no++;
            }

            var result = data.ToPagedList(pageNumber, pageSize);

            ViewBag.Count = result.TotalItemCount;

            return View(result);
        }

        // ================= EDIT =================
        [HttpGet]
        public IActionResult Edit(string id) {
            var user = db.Users.FirstOrDefault(x => x.Id == id);

            if (user == null)
                return NotFound();

            var address = db.Addresses
                .FirstOrDefault(x => x.UserId == user.Id);

            var model = new EditUserVM {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                

                ReceiverName = address?.ReceiverName,
                Phone = address?.Phone,
                FullAddress = address?.FullAddress
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserVM model) {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
                return NotFound();

            user.UserName = string.IsNullOrWhiteSpace(model.UserName)
                ? user.Email
                : model.UserName.Trim();

            user.Email = model.Email?.Trim(); 
            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded) {
                foreach (var error in result.Errors) {
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

            TempData["Success"] = "User updated successfully";

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        [HttpPost]
        public async Task<IActionResult> Delete(string id) {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) {
                return Json(new {
                    success = false,
                    message = "User not found"
                });
            }

            bool hasOrders = db.Orders.Any(x => x.UserId == id);

            if (hasOrders) {
                return Json(new {
                    success = false,
                    message = "Cannot delete this user because this user already has orders."
                });
            }

            var addresses = db.Addresses.Where(x => x.UserId == id);
            db.Addresses.RemoveRange(addresses);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded) {
                return Json(new {
                    success = false,
                    message = result.Errors.FirstOrDefault()?.Description ?? "Delete failed"
                });
            }

            await db.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}