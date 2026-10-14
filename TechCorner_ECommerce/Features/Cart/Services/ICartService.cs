using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface ICartService {
        CartOperationResult AddToCart(List<CartItemVM> cart, int productId, int quantity);
        CartOperationResult RemoveFromCart(List<CartItemVM> cart, int productId);
        CartOperationResult UpdateQuantity(List<CartItemVM> cart, int productId, int quantity);
        int GetQuantity(IEnumerable<CartItemVM> cart);
        decimal GetSubTotal(IEnumerable<CartItemVM> cart);
    }
}
