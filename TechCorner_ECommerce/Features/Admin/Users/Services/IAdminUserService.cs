using System.Security.Claims;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IAdminUserService {
        Task<AdminUserIndexResult> GetUsersAsync(string? keyword, int pageNumber, int pageSize);
        Task<AdminUserOperationResult> CanEditUserAsync(ClaimsPrincipal principal, string id);
        Task<EditUserVM?> GetEditUserAsync(string id);
        Task<AdminUserOperationResult> EditUserAsync(EditUserVM model);
        Task<AdminUserOperationResult> DeleteUserAsync(ClaimsPrincipal principal, string id);
        List<string> GetRoleNames();
    }
}
