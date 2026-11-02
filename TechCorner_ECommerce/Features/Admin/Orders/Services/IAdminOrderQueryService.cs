using TechCorner_ECommerce.Models.Enums;
using TechCorner_ECommerce.ViewModels;
using X.PagedList;

namespace TechCorner_ECommerce.Services {
    public interface IAdminOrderQueryService {
        IPagedList<AdminOrderVM> GetOrders(string? keyword, OrderStatus? status, PaymentStatus? paymentStatus, int pageNumber, int pageSize);
        Task<AdminOrderDetailVM?> GetOrderDetailAsync(string orderCode);
    }
}
