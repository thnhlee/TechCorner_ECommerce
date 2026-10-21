using Microsoft.EntityFrameworkCore;
using TechCorner_ECommerce.Data;
using TechCorner_ECommerce.Models.Enums;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public class AdminDashboardService : IAdminDashboardService {
        private const int LowStockThreshold = 10;
        private const string FallbackProductImage = "/Admin/assets/images/product-1.png";

        private readonly AppDbContext db;

        public AdminDashboardService(AppDbContext context) {
            db = context;
        }

        ///////////////////////// DASHBOARD SUMMARY /////////////////////////
        public async Task<DashboardVM> GetDashboardAsync() {
            var today = DateTime.Today;
            var currentMonthStart = new DateTime(today.Year, today.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var nextMonthStart = currentMonthStart.AddMonths(1);

            var totalSales = await GetRevenueAsync(null, null);
            var currentMonthSales = await GetRevenueAsync(currentMonthStart, nextMonthStart);
            var previousMonthSales = await GetRevenueAsync(previousMonthStart, currentMonthStart);

            var totalOrders = await db.Orders.CountAsync();
            var currentMonthOrders = await db.Orders.CountAsync(x => x.OrderDate >= currentMonthStart && x.OrderDate < nextMonthStart);
            var previousMonthOrders = await db.Orders.CountAsync(x => x.OrderDate >= previousMonthStart && x.OrderDate < currentMonthStart);

            var totalProducts = await db.Products.CountAsync();
            var currentMonthProducts = await db.Products.CountAsync(x => x.CreatedAt >= currentMonthStart && x.CreatedAt < nextMonthStart);
            var previousMonthProducts = await db.Products.CountAsync(x => x.CreatedAt >= previousMonthStart && x.CreatedAt < currentMonthStart);

            var lowStockCount = await db.Products.CountAsync(x => x.StockQuantity <= LowStockThreshold);
            var outOfStockCount = await db.Products.CountAsync(x => x.StockQuantity <= 0);

            return new DashboardVM {
                TotalSales = new DashboardMetricVM {
                    Label = "Total Sales",
                    Value = FormatCurrency(totalSales),
                    ChangeText = FormatChange(currentMonthSales, previousMonthSales),
                    IsPositiveChange = currentMonthSales >= previousMonthSales
                },
                TotalOrders = new DashboardMetricVM {
                    Label = "Total Orders",
                    Value = totalOrders.ToString("N0"),
                    ChangeText = FormatChange(currentMonthOrders, previousMonthOrders),
                    IsPositiveChange = currentMonthOrders >= previousMonthOrders
                },
                TotalProducts = new DashboardMetricVM {
                    Label = "Product Variants",
                    Value = totalProducts.ToString("N0"),
                    ChangeText = FormatChange(currentMonthProducts, previousMonthProducts),
                    IsPositiveChange = currentMonthProducts >= previousMonthProducts
                },
                LowStock = new DashboardMetricVM {
                    Label = "Low Stock",
                    Value = lowStockCount.ToString("N0"),
                    ChangeText = $"{outOfStockCount:N0} out of stock",
                    IsPositiveChange = outOfStockCount == 0
                },
                DeliveredRevenue = await GetRevenueByStatusAsync(OrderStatus.Delivered),
                PendingRevenue = await GetRevenueByStatusAsync(OrderStatus.Pending),
                CancelledRevenue = await GetRevenueByStatusAsync(OrderStatus.Cancelled),
                TotalCustomers = await GetCustomerCountAsync(),
                FirstTimeCustomers = await GetFirstTimeCustomerCountAsync(),
                ReturningCustomers = await GetReturningCustomerCountAsync(),
                TotalProductVariants = totalProducts,
                MonthlySales = await GetMonthlySalesAsync(today),
                TopSellingProducts = await GetTopSellingProductsAsync(),
                LowStockProducts = await GetLowStockProductsAsync(),
                RecentOrders = await GetRecentOrdersAsync()
            };
        }

        ///////////////////////// METRICS /////////////////////////
        private async Task<decimal> GetRevenueAsync(DateTime? from, DateTime? to) {
            var orders = db.Orders
                .Where(x => x.Status != OrderStatus.Cancelled)
                .AsQueryable();

            if (from.HasValue) {
                orders = orders.Where(x => x.OrderDate >= from.Value);
            }

            if (to.HasValue) {
                orders = orders.Where(x => x.OrderDate < to.Value);
            }

            return await orders.SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;
        }

        private async Task<decimal> GetRevenueByStatusAsync(OrderStatus status) {
            return await db.Orders
                .Where(x => x.Status == status)
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;
        }

        private async Task<int> GetCustomerCountAsync() {
            var registeredUsers = await db.Users.CountAsync();
            var guestCustomers = await db.Orders
                .Where(x => x.UserId == null && x.ReceiverEmail != "")
                .Select(x => x.ReceiverEmail)
                .Distinct()
                .CountAsync();

            return registeredUsers + guestCustomers;
        }

        private async Task<int> GetFirstTimeCustomerCountAsync() {
            return await db.Orders
                .GroupBy(x => x.ReceiverEmail)
                .Where(x => x.Count() == 1)
                .CountAsync();
        }

        private async Task<int> GetReturningCustomerCountAsync() {
            return await db.Orders
                .GroupBy(x => x.ReceiverEmail)
                .Where(x => x.Count() > 1)
                .CountAsync();
        }

        ///////////////////////// CHART DATA /////////////////////////
        private async Task<List<DashboardMonthlySalesVM>> GetMonthlySalesAsync(DateTime today) {
            var startMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
            var endMonth = startMonth.AddMonths(6);

            var sales = await db.Orders
                .Where(x => x.OrderDate >= startMonth && x.OrderDate < endMonth && x.Status != OrderStatus.Cancelled)
                .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
                .Select(x => new {
                    x.Key.Year,
                    x.Key.Month,
                    Revenue = x.Sum(o => o.TotalPrice),
                    OrderCount = x.Count()
                })
                .ToListAsync();

            return Enumerable.Range(0, 6)
                .Select(index => {
                    var month = startMonth.AddMonths(index);
                    var monthSales = sales.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month);

                    return new DashboardMonthlySalesVM {
                        MonthLabel = month.ToString("MMM yyyy"),
                        Revenue = monthSales?.Revenue ?? 0m,
                        OrderCount = monthSales?.OrderCount ?? 0
                    };
                })
                .ToList();
        }

        ///////////////////////// PRODUCT LISTS /////////////////////////
        private async Task<List<DashboardProductSummaryVM>> GetTopSellingProductsAsync() {
            var soldProducts = await db.OrderDetails
                .Where(x => x.Order.Status != OrderStatus.Cancelled)
                .GroupBy(x => x.ProductId)
                .Select(x => new {
                    ProductId = x.Key,
                    UnitsSold = x.Sum(i => i.Quantity),
                    Revenue = x.Sum(i => i.Price * i.Quantity)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(5)
                .ToListAsync();

            var products = await GetProductsByIdAsync(soldProducts.Select(x => x.ProductId).ToList());
            var totalUnitsSold = soldProducts.Sum(x => x.UnitsSold);

            return soldProducts
                .Select(item => {
                    var product = products.GetValueOrDefault(item.ProductId);

                    return new DashboardProductSummaryVM {
                        ProductId = item.ProductId,
                        ProductName = product?.ProductName ?? "Product unavailable",
                        SkuCode = product?.SkuCode ?? "",
                        ImageUrl = product?.ImageUrl ?? FallbackProductImage,
                        Price = product?.Price ?? 0m,
                        StockQuantity = product?.StockQuantity ?? 0,
                        UnitsSold = item.UnitsSold,
                        Revenue = item.Revenue,
                        SalesSharePercent = totalUnitsSold == 0 ? 0 : Math.Round(item.UnitsSold * 100m / totalUnitsSold, 1)
                    };
                })
                .ToList();
        }

        private async Task<List<DashboardProductSummaryVM>> GetLowStockProductsAsync() {
            return await db.Products
                .Include(x => x.ParentProduct)
                    .ThenInclude(x => x.Images)
                .OrderBy(x => x.StockQuantity)
                .Take(5)
                .Select(x => new DashboardProductSummaryVM {
                    ProductId = x.Id,
                    ProductName = x.ParentProduct.Name,
                    SkuCode = x.SkuCode,
                    ImageUrl = x.ParentProduct.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                        ?? x.ParentProduct.Images
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()
                        ?? FallbackProductImage,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity
                })
                .ToListAsync();
        }

        private async Task<Dictionary<int, DashboardProductSummaryVM>> GetProductsByIdAsync(List<int> productIds) {
            if (productIds.Count == 0) {
                return new Dictionary<int, DashboardProductSummaryVM>();
            }

            return await db.Products
                .Include(x => x.ParentProduct)
                    .ThenInclude(x => x.Images)
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new DashboardProductSummaryVM {
                    ProductId = x.Id,
                    ProductName = x.ParentProduct.Name,
                    SkuCode = x.SkuCode,
                    ImageUrl = x.ParentProduct.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                        ?? x.ParentProduct.Images
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()
                        ?? FallbackProductImage,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity
                })
                .ToDictionaryAsync(x => x.ProductId);
        }

        ///////////////////////// RECENT ORDERS /////////////////////////
        private async Task<List<DashboardRecentOrderVM>> GetRecentOrdersAsync() {
            var orders = await db.Orders
                .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Product!)
                    .ThenInclude(x => x.ParentProduct)
                    .ThenInclude(x => x.Images)
                .OrderByDescending(x => x.OrderDate)
                .Take(5)
                .ToListAsync();

            return orders.Select(order => {
                var firstItem = order.OrderDetails.FirstOrDefault();
                var product = firstItem?.Product;
                var parent = product?.ParentProduct;

                var imageUrl = parent?.Images
                    .Where(x => x.IsPrimary)
                    .Select(x => x.ImageUrl)
                    .FirstOrDefault()
                    ?? parent?.Images.Select(x => x.ImageUrl).FirstOrDefault()
                    ?? FallbackProductImage;

                return new DashboardRecentOrderVM {
                    OrderCode = order.OrderCode,
                    CustomerName = order.ReceiverName,
                    ProductName = parent?.Name ?? "Order item unavailable",
                    ImageUrl = imageUrl,
                    TotalPrice = order.TotalPrice,
                    ItemCount = order.OrderDetails.Sum(x => x.Quantity),
                    Status = order.Status,
                    OrderDate = order.OrderDate
                };
            }).ToList();
        }

        ///////////////////////// FORMATTERS /////////////////////////
        private static string FormatCurrency(decimal value) {
            return $"{value:N0}đ";
        }

        private static string FormatChange(decimal currentValue, decimal previousValue) {
            if (previousValue == 0) {
                return currentValue > 0 ? "+100% since last month" : "0% since last month";
            }

            var percent = Math.Round((currentValue - previousValue) * 100m / previousValue, 1);
            return $"{percent:+0.#;-0.#;0}% since last month";
        }

        private static string FormatChange(int currentValue, int previousValue) {
            if (previousValue == 0) {
                return currentValue > 0 ? "+100% since last month" : "0% since last month";
            }

            var percent = Math.Round((currentValue - previousValue) * 100m / previousValue, 1);
            return $"{percent:+0.#;-0.#;0}% since last month";
        }
    }
}
