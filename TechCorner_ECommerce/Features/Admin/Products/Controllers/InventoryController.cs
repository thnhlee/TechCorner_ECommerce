using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;

namespace TechCorner_ECommerce.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class InventoryController : Controller {
        private readonly IInventoryService inventoryService;

        public InventoryController(IInventoryService inventoryService) {
            this.inventoryService = inventoryService;
        }

        ///////////////////////// EXPORT EXCEL /////////////////////////
        public async Task<IActionResult> ExportToExcel() {
            var result = await inventoryService.ExportToExcelAsync();

            return File(result.Stream, result.ContentType, result.FileName);
        }

        ///////////////////////// INVENTORY /////////////////////////
        public IActionResult Index(int? cate, string? keyword, int? page) {
            const int pageSize = 3;
            var pageNumber = page ?? 1;

            ViewBag.Cate = cate;
            ViewBag.SearchQuery = keyword;

            var result = inventoryService.GetInventory(cate, keyword, pageNumber, pageSize);

            ViewBag.SubCategories = result.SubCategories;
            ViewBag.Count = result.Count;

            return View(result.Items);
        }
    }
}
