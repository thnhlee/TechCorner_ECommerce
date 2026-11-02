using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class DashboardController : Controller {
        private readonly IAdminDashboardService dashboardService;

        public DashboardController(IAdminDashboardService dashboardService) {
            this.dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index() {
            var model = await dashboardService.GetDashboardAsync();
            return View(model);
        }
    }
}
