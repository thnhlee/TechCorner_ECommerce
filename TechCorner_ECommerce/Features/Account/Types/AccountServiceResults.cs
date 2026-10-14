using Microsoft.AspNetCore.Identity;

namespace TechCorner_ECommerce.ViewModels {
    public class AccountLoginResult {
        public bool Succeeded { get; set; }
        public bool IsAdminArea { get; set; }
        public bool ReturnToCart { get; set; }
        public bool IsNotAllowed { get; set; }
        public string ErrorMessage { get; set; } = "";
    }

    public class AccountOperationResult {
        public bool Succeeded { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public IEnumerable<IdentityError> Errors { get; set; } = Enumerable.Empty<IdentityError>();
    }
}
