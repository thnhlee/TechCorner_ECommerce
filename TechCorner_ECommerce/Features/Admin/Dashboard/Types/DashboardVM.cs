using TechCorner_ECommerce.Models.Enums;

namespace TechCorner_ECommerce.ViewModels {
    public class DashboardVM {
        public DashboardMetricVM TotalSales { get; set; } = new();
        public DashboardMetricVM TotalOrders { get; set; } = new();
        public DashboardMetricVM TotalProducts { get; set; } = new();
        public DashboardMetricVM LowStock { get; set; } = new();

        public decimal DeliveredRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public decimal CancelledRevenue { get; set; }

        public int TotalCustomers { get; set; }
        public int FirstTimeCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public int TotalProductVariants { get; set; }

        public List<DashboardMonthlySalesVM> MonthlySales { get; set; } = new();
        public List<DashboardProductSummaryVM> TopSellingProducts { get; set; } = new();
        public List<DashboardProductSummaryVM> LowStockProducts { get; set; } = new();
        public List<DashboardRecentOrderVM> RecentOrders { get; set; } = new();
    }

    public class DashboardMetricVM {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string ChangeText { get; set; } = "";
        public bool IsPositiveChange { get; set; }
    }

    public class DashboardMonthlySalesVM {
        public string MonthLabel { get; set; } = "";
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class DashboardProductSummaryVM {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string SkuCode { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal SalesSharePercent { get; set; }
    }

    public class DashboardRecentOrderVM {
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public int ItemCount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
