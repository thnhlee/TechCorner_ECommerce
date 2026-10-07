using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Services {
    public class AdminUserService : IAdminUserService {
        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminUserService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager) {
            db = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<AdminUserIndexResult> GetUsersAsync(string? keyword, int pageNumber, int pageSize) {
            var users = db.Users
                .GroupJoin(
                    db.Addresses,
                    user => user.Id,
                    address => address.UserId,
                    (user, addresses) => new {
                        User = user,
                        Address = addresses.FirstOrDefault()
                    })
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword)) {
                keyword = keyword.Trim();

                users = users.Where(x =>
                    (x.User.UserName != null && x.User.UserName.Contains(keyword)) ||
                    (x.User.Email != null && x.User.Email.Contains(keyword)) ||
                    (x.Address != null && x.Address.Phone != null && x.Address.Phone.Contains(keyword)) ||
                    (x.Address != null && x.Address.FullAddress != null && x.Address.FullAddress.Contains(keyword)));
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

            var no = 1;
            foreach (var item in data) {
                item.No = no++;

                // Role lookup stays here because UserManager owns Identity role membership.
                var user = db.Users.FirstOrDefault(x => x.Id == item.Id);

                if (user != null) {
                    var roles = await userManager.GetRolesAsync(user);
                    item.Role = roles.FirstOrDefault() ?? "No Role";
                }
            }

            var result = data.ToPagedList(pageNumber, pageSize);

            return new AdminUserIndexResult {
                Items = result,
                Count = result.TotalItemCount
            };
        }

        public Task<AdminUserOperationResult> CanEditUserAsync(ClaimsPrincipal principal, string id) {
            var currentUserId = userManager.GetUserId(principal);

            // Admins should not edit their own account from the admin user editor.
            return Task.FromResult(id == currentUserId
                ? AdminUserOperationResult.Denied("You cannot edit your own account.")
                : AdminUserOperationResult.Success());
        }

        public async Task<EditUserVM?> GetEditUserAsync(string id) {
            var user = db.Users.FirstOrDefault(x => x.Id == id);

            if (user == null) {
                return null;
            }

            var address = db.Addresses.FirstOrDefault(x => x.UserId == user.Id);
            var currentRoles = await userManager.GetRolesAsync(user);

            return new EditUserVM {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ReceiverName = address?.ReceiverName,
                Phone = address?.Phone,
                FullAddress = address?.FullAddress,
                RoleName = currentRoles.FirstOrDefault(),
                Roles = GetRoleNames()
            };
        }

        public async Task<AdminUserOperationResult> EditUserAsync(EditUserVM model) {
            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null) {
                return AdminUserOperationResult.Missing();
            }

            user.UserName = string.IsNullOrWhiteSpace(model.UserName)
                ? user.Email
                : model.UserName.Trim();
            user.Email = model.Email?.Trim();
            user.UpdatedAt = DateTime.Now;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded) {
                return new AdminUserOperationResult {
                    Errors = result.Errors
                };
            }

            var address = db.Addresses.FirstOrDefault(x => x.UserId == user.Id);

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

            var currentRoles = await userManager.GetRolesAsync(user);

            if (currentRoles.Any()) {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            if (!string.IsNullOrWhiteSpace(model.RoleName)) {
                await userManager.AddToRoleAsync(user, model.RoleName);
            }

            await db.SaveChangesAsync();

            return AdminUserOperationResult.Success("User updated successfully");
        }

        public async Task<AdminUserOperationResult> DeleteUserAsync(ClaimsPrincipal principal, string id) {
            var currentUserId = userManager.GetUserId(principal);

            // Prevent deleting the currently signed-in admin account.
            if (id == currentUserId) {
                return AdminUserOperationResult.Failure("You cannot delete your own account.");
            }

            var user = await userManager.FindByIdAsync(id);

            if (user == null) {
                return AdminUserOperationResult.Failure("User not found");
            }

            var roles = await userManager.GetRolesAsync(user);

            // Keep privileged accounts from being removed through this screen.
            if (roles.Any(r => r == "Admin" || r == "Staff")) {
                return AdminUserOperationResult.Failure("Cannot delete this account.");
            }

            var hasOrders = db.Orders.Any(x => x.UserId == id);

            if (hasOrders) {
                return AdminUserOperationResult.Failure("Cannot delete this user because this user already has orders.");
            }

            var addresses = db.Addresses.Where(x => x.UserId == id);
            db.Addresses.RemoveRange(addresses);

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded) {
                return AdminUserOperationResult.Failure(result.Errors.FirstOrDefault()?.Description ?? "Delete failed");
            }

            await db.SaveChangesAsync();

            return AdminUserOperationResult.Success();
        }

        public List<string> GetRoleNames() {
            return roleManager.Roles
                .Where(x => x.Name != null)
                .Select(x => x.Name!)
                .ToList();
        }
    }
}
