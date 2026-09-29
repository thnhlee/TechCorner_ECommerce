using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using TechCorner_ECommerce.Models;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Controllers {
    public class AccountController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService emailService;
        private readonly EmailSettings emailSettings;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService, IOptions<EmailSettings> emailOptions) {
            _userManager = userManager;
            _signInManager = signInManager;
            this.emailService = emailService;
            emailSettings = emailOptions.Value;
        }

        [HttpGet]
        public IActionResult AccessDenied() {
            return View();
        }

        ///////////////////////// LOGIN  /////////////////////////
        [HttpGet]
        public IActionResult Login() {
            if (User.Identity != null && User.Identity.IsAuthenticated) {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnToCart) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) {
                ModelState.AddModelError(string.Empty, "Email or Password is incorrect");
                return View(model);
            }

            var userName = user.UserName ?? user.Email;

            if (string.IsNullOrWhiteSpace(userName)) {
                ModelState.AddModelError(string.Empty, "Email or Password is incorrect");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded) {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin") || roles.Contains("Staff")) {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                if (!string.IsNullOrEmpty(returnToCart)) {
                    return Redirect(returnToCart);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsNotAllowed) {
                ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email or Password is incorrect");
            return View(model);
        }

        ///////////////////////// REGISTER  /////////////////////////
        [HttpGet]
        public IActionResult Register() {
            if (User.Identity != null && User.Identity.IsAuthenticated) {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var email = model.Email.Trim();
            var user = new ApplicationUser {
                UserName = email,
                Email = email,
                EmailConfirmed = !emailSettings.Enabled,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded) {
                if (emailSettings.Enabled) {
                    await SendConfirmationEmailAsync(user);
                }

                TempData["SuccessMessage"] = "Registration successful. Please check your email to confirm your account.";

                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        ///////////////////////// Email Confirmation  /////////////////////////
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code) {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code)) {
                TempData["ErrorMessage"] = "Invalid email confirmation link.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) {
                TempData["ErrorMessage"] = "Invalid email confirmation link.";
                return RedirectToAction(nameof(Login));
            }

            string token;

            try {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch {
                TempData["ErrorMessage"] = "Invalid email confirmation link.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);


            if (result.Succeeded) {
                TempData["SuccessMessage"] = "Email confirmed successfully. You can now log in.";
            }
            else {
                TempData["ErrorMessage"] = "Could not confirm your email. Please try again.";
            }


            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgotPassword() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());

            if (user != null && !string.IsNullOrWhiteSpace(user.Email)) {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var resetUrl = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { email = user.Email, code },
                    Request.Scheme);

                if (!string.IsNullOrWhiteSpace(resetUrl)) {
                    var body = BuildAuthEmail(
                        "Reset your password",
                        "We received a request to reset your password.",
                        "Reset password",
                        resetUrl);

                    await emailService.SendAsync(user.Email, "Reset your  password", body);
                }
            }

            TempData["SuccessMessage"] = "A password reset link has been sent.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string code) {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code)) {
                TempData["ErrorMessage"] = "Invalid password reset link.";
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordVM {
                Email = email,
                Code = code
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) {
                TempData["SuccessMessage"] = "Password reset completed.";
                return RedirectToAction(nameof(Login));
            }

            string token;

            try {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            }
            catch {
                ModelState.AddModelError(string.Empty, "Invalid password reset link.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded) {
                TempData["SuccessMessage"] = "Password reset completed. You can now log in.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout() {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task SendConfirmationEmailAsync(ApplicationUser user) {
            if (string.IsNullOrWhiteSpace(user.Email)) {
                return;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId = user.Id, code },
                Request.Scheme);

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
