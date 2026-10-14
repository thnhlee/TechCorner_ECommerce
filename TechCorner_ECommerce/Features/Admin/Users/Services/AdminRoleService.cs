using Microsoft.AspNetCore.Identity;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;
using X.PagedList.Extensions;

namespace TechCorner_ECommerce.Services {
    public class AdminRoleService : IAdminRoleService {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AdminRoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        ///////////////////////// ROLE LIST /////////////////////////
        public async Task<AdminRoleIndexResult> GetRolesAsync(string? keyword, int pageNumber, int pageSize) {
            var roles = roleManager.Roles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword)) {
                keyword = keyword.Trim();
                roles = roles.Where(x => x.Name != null && x.Name.Contains(keyword));
            }

            var data = roles
                .OrderByDescending(x => x.Id)
                .Select(x => new RoleVM {
                    Id = x.Id,
                    Name = x.Name ?? ""
                })
                .ToList();

            var no = 1;

            foreach (var item in data) {
                item.No = no++;

                if (!string.IsNullOrWhiteSpace(item.Name)) {
                    item.UserCount = (await userManager.GetUsersInRoleAsync(item.Name)).Count;
                }
            }

            var result = data.ToPagedList(pageNumber, pageSize);

            return new AdminRoleIndexResult {
                Items = result,
                Count = result.Count
            };
        }

        ///////////////////////// ROLE CREATE /////////////////////////
        public async Task<AdminRoleOperationResult> CreateRoleAsync(RoleCreateVM model) {
            if (string.IsNullOrWhiteSpace(model.Name)) {
                return AdminRoleOperationResult.Failure("Role name is required", "Name");
            }

            var roleName = model.Name.Trim();
            var exists = await roleManager.RoleExistsAsync(roleName);

            if (exists) {
                return AdminRoleOperationResult.Failure("Role already exists", "Name");
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));

            return result.Succeeded
                ? AdminRoleOperationResult.Success()
                : new AdminRoleOperationResult {
                    Errors = result.Errors
                };
        }

        ///////////////////////// ROLE EDIT /////////////////////////
        public async Task<RoleEditVM?> GetEditRoleAsync(string id) {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null) {
                return null;
            }

            return new RoleEditVM {
                Id = role.Id,
                Name = role.Name ?? ""
            };
        }

        public async Task<AdminRoleOperationResult> EditRoleAsync(RoleEditVM model) {
            var role = await roleManager.FindByIdAsync(model.Id);

            if (role == null) {
                return AdminRoleOperationResult.Missing();
            }

            var roleName = model.Name.Trim();
            var duplicate = roleManager.Roles.Any(x => x.Id != model.Id && x.Name == roleName);

            if (duplicate) {
                return AdminRoleOperationResult.Failure("Role already exists", "Name");
            }

            role.Name = roleName;
            role.NormalizedName = roleName.ToUpper();

            var result = await roleManager.UpdateAsync(role);

            return result.Succeeded
                ? AdminRoleOperationResult.Success()
                : new AdminRoleOperationResult {
                    Errors = result.Errors
                };
        }

        ///////////////////////// ROLE DELETE /////////////////////////
        public async Task<AdminRoleOperationResult> DeleteRoleAsync(string id) {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null) {
                return AdminRoleOperationResult.Missing("Role not found");
            }

            var users = string.IsNullOrWhiteSpace(role.Name)
                ? new List<ApplicationUser>()
                : await userManager.GetUsersInRoleAsync(role.Name);

            if (users.Any()) {
                return AdminRoleOperationResult.Failure("Cannot delete this role because it currently in used.");
            }

            var result = await roleManager.DeleteAsync(role);

            if (!result.Succeeded) {
                return AdminRoleOperationResult.Failure(result.Errors.FirstOrDefault()?.Description ?? "Delete failed");
            }

            return AdminRoleOperationResult.Success();
        }
    }
}
