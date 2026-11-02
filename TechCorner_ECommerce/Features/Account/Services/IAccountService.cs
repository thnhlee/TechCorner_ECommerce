using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public interface IAccountService {
        Task<AccountLoginResult> LoginAsync(LoginVM model, bool hasReturnToCart);
        Task<AccountOperationResult> RegisterAsync(RegisterVM model, Func<string, string, string?> confirmationUrlFactory);
        Task<AccountOperationResult> ConfirmEmailAsync(string userId, string code);
        Task RequestPasswordResetAsync(ForgotPasswordVM model, Func<string, string, string?> resetUrlFactory);
        Task<AccountOperationResult> ResetPasswordAsync(ResetPasswordVM model);
        Task LogoutAsync();
    }
}
