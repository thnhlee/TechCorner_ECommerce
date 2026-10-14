using TechCorner_ECommerce.Models;

namespace TechCorner_ECommerce.ViewModels {
    public class OrderValidationError {
        public string Key { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class CheckoutPageResult {
        public bool CartEmpty { get; set; }
        public CheckoutVM Model { get; set; } = new();
    }

    public class PlaceOrderResult {
        public bool Succeeded { get; set; }
        public bool CartEmpty { get; set; }
        public string? OrderCode { get; set; }
        public CheckoutVM Model { get; set; } = new();
        public List<OrderValidationError> Errors { get; set; } = new();
    }

    public class UserOrderResult<T> {
        public bool RequiresLogin { get; set; }
        public T? Value { get; set; }
    }

    public class OrderSuccessResult {
        public Order? Order { get; set; }
    }
}
