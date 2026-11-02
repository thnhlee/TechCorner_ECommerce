using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IAdminRoleService {
        Task<AdminRoleIndexResult> GetRolesAsync(string? keyword, int pageNumber, int pageSize);
        Task<AdminRoleOperationResult> CreateRoleAsync(RoleCreateVM model);
        Task<RoleEditVM?> GetEditRoleAsync(string id);
        Task<AdminRoleOperationResult> EditRoleAsync(RoleEditVM model);
        Task<AdminRoleOperationResult> DeleteRoleAsync(string id);
    }
}
