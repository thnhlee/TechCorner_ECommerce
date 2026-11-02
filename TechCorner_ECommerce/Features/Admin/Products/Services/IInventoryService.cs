using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IInventoryService {
        Task<InventoryExportResult> ExportToExcelAsync();
        InventoryIndexResult GetInventory(int? categoryId, string? keyword, int pageNumber, int pageSize);
    }
}
