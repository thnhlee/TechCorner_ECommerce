using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Services {
    public class AccountService : IAccountService {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailService emailService;
        private readonly EmailSettings emailSettings;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IOptions<EmailSettings> emailOptions) {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            emailSettings = emailOptions.Value;
        }

        public async Task<AccountLoginResult> LoginAsync(LoginVM model, bool hasReturnToCart) {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null) {
                return InvalidLogin();
            }

            var userName = user.UserName ?? user.Email;

            if (string.IsNullOrWhiteSpace(userName)) {
                return InvalidLogin();
            }

            var result = await signInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded) {
                return new AccountLoginResult {
                    IsNotAllowed = result.IsNotAllowed,
                    ErrorMessage = result.IsNotAllowed
                        ? "Please confirm your email before logging in."
                        : "Email or Password is incorrect"
                };
            }

            var roles = await userManager.GetRolesAsync(user);

            return new AccountLoginResult {
                Succeeded = true,
                IsAdminArea = roles.Contains("Admin") || roles.Contains("Staff"),
                ReturnToCart = hasReturnToCart
            };
        }

        public async Task<AccountOperationResult> RegisterAsync(RegisterVM model, Func<string, string, string?> confirmationUrlFactory) {
            var email = model.Email.Trim();
            var user = new ApplicationUser {
                UserName = email,
                Email = email,
                // When email sending is disabled, the account can log in immediately.
                EmailConfirmed = !emailSettings.Enabled,
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) {
                return new AccountOperationResult {
                    Errors = result.Errors
                };
            }

            if (emailSettings.Enabled) {
                await SendConfirmationEmailAsync(user, confirmationUrlFactory);
            }

            return new AccountOperationResult {
                Succeeded = true,
                SuccessMessage = "Registration successful. Please check your email to confirm your account."
            };
        }

        public async Task<AccountOperationResult> ConfirmEmailAsync(string userId, string code) {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code)) {
                return InvalidEmailConfirmation();
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null) {
                return InvalidEmailConfirmation();
            }

            string token;

            try {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch {
                return InvalidEmailConfirmation();
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            return result.Succeeded
                ? new AccountOperationResult {
                    Succeeded = true,
                    SuccessMessage = "Email confirmed successfully. You can now log in."
                }
                : new AccountOperationResult {
                    ErrorMessage = "Could not confirm your email. Please try again."
                };
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordVM model, Func<string, string, string?> resetUrlFactory) {
            var user = await userManager.FindByEmailAsync(model.Email.Trim());

            // Do not reveal whether the email exists.
            if (user == null || string.IsNullOrWhiteSpace(user.Email)) {
                return;
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = resetUrlFactory(user.Email, code);

            if (string.IsNullOrWhiteSpace(resetUrl)) {
                return;
            }

            var body = BuildAuthEmail(
                "Reset your password",
                "We received a request to reset your password.",
                "Reset password",
                resetUrl);

            await emailService.SendAsync(user.Email, "Reset your  password", body);
        }

        public async Task<AccountOperationResult> ResetPasswordAsync(ResetPasswordVM model) {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null) {
                return new AccountOperationResult {
                    Succeeded = true,
                    SuccessMessage = "Password reset completed."
                };
            }

            string token;

            try {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            }
            catch {
                return new AccountOperationResult {
                    ErrorMessage = "Invalid password reset link."
                };
            }

            var result = await userManager.ResetPasswordAsync(user, token, model.Password);

            return result.Succeeded
                ? new AccountOperationResult {
                    Succeeded = true,
                    SuccessMessage = "Password reset completed. You can now log in."
                }
                : new AccountOperationResult {
                    Errors = result.Errors
                };
        }

        public async Task LogoutAsync() {
            await signInManager.SignOutAsync();
        }

        private async Task SendConfirmationEmailAsync(ApplicationUser user, Func<string, string, string?> confirmationUrlFactory) {
            if (string.IsNullOrWhiteSpace(user.Email)) {
                return;
            }

            // Identity tokens are encoded so they can safely travel inside a URL.
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = confirmationUrlFactory(user.Id, code);

            if (string.IsNullOrWhiteSpace(confirmationUrl)) {
                return;
            }

            var body = BuildAuthEmail(
                "Confirm your email",
                "Thanks for creating a TechCorner account. Please confirm your email.",
                "Confirm email",
                confirmationUrl);

            await emailService.SendAsync(user.Email, "Confirm your TechCorner email", body);
        }

        private static AccountLoginResult InvalidLogin() {
            return new AccountLoginResult {
                ErrorMessage = "Email or Password is incorrect"
            };
        }

        private static AccountOperationResult InvalidEmailConfirmation() {
            return new AccountOperationResult {
                ErrorMessage = "Invalid email confirmation link."
            };
        }

        private static string BuildAuthEmail(string title, string message, string buttonText, string url) {
            var safeTitle = HtmlEncoder.Default.Encode(title);
            var safeMessage = HtmlEncoder.Default.Encode(message);
            var safeButtonText = HtmlEncoder.Default.Encode(buttonText);
            var safeUrl = HtmlEncoder.Default.Encode(url);

            return $"""
                <div style="font-family:Arial,sans-serif;max-width:640px;margin:auto;color:#111827">
                    <h2>{safeTitle}</h2>
                    <p>{safeMessage}</p>
                    <p>
                        <a href="{safeUrl}" style="display:inline-block;background:#dbcc8f;color:#111827;padding:12px 18px;border-radius:6px;text-decoration:none;font-weight:700">
                            {safeButtonText}
                        </a>
                    </p>
                    <p style="color:#6b7280;font-size:13px">If the button does not work, copy this link into your browser:</p>
                    <p style="word-break:break-all;color:#374151">{safeUrl}</p>
                </div>
                """;
        }
    }
}
