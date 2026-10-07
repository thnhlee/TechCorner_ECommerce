using System.Security.Claims;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IOrderService {
        Task<CheckoutPageResult> GetCheckoutAsync(ClaimsPrincipal principal, List<CartItemVM> cart);
        CheckoutVM PrepareCheckoutSubmission(CheckoutVM model, List<CartItemVM> cart);
        IReadOnlyList<OrderValidationError> ValidateCheckout(CheckoutVM model);
        Task<PlaceOrderResult> PlaceOrderAsync(ClaimsPrincipal principal, CheckoutVM model);
        Task<UserOrderResult<List<OrderListVM>>> GetUserOrdersAsync(ClaimsPrincipal principal);
        Task<UserOrderResult<Order>> GetUserOrderDetailAsync(ClaimsPrincipal principal, string orderCode);
        Task<Order?> GetSuccessOrderAsync(ClaimsPrincipal principal, string orderCode);
    }
}
