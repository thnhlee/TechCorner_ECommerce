using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IAdminDashboardService {
        Task<DashboardVM> GetDashboardAsync();
    }
}
