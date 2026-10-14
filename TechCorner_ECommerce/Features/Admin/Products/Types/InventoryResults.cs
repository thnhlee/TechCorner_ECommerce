using X.PagedList;

namespace TechCorner_ECommerce.ViewModels {
    public class InventoryIndexResult {
        public IPagedList<ProductVM>? Items { get; set; }
        public List<Models.SubCategory> SubCategories { get; set; } = new();
        public int Count { get; set; }
    }

    public class InventoryExportResult {
        public MemoryStream Stream { get; set; } = new();
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }
}
