using Microsoft.AspNetCore.Mvc;
using TechCorner_ECommerce.Services;
using TechCorner_ECommerce.ViewModels;

namespace TechCorner_ECommerce.Controllers {
    public class AccountController : Controller {
        private readonly IAccountService accountService;

        public AccountController(IAccountService accountService) {
            this.accountService = accountService;
        }

        ///////////////////////// ACCESS DENIED /////////////////////////
        [HttpGet]
        public IActionResult AccessDenied() {
            return View();
        }

        ///////////////////////// LOGIN /////////////////////////
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

            var result = await accountService.LoginAsync(model, !string.IsNullOrEmpty(returnToCart));

            if (result.Succeeded) {
                if (result.IsAdminArea) {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                if (result.ReturnToCart) {
                    return Redirect(returnToCart!);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(model);
        }

        ///////////////////////// REGISTER /////////////////////////
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

            var result = await accountService.RegisterAsync(model, (userId, code) =>
                Url.Action(nameof(ConfirmEmail), "Account", new { userId, code }, Request.Scheme));

            if (result.Succeeded) {
                TempData["SuccessMessage"] = result.SuccessMessage;
                return RedirectToAction(nameof(Login));
            }

            AddIdentityErrors(result);
            return View(model);
        }

        ///////////////////////// EMAIL CONFIRMATION /////////////////////////
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code) {
            var result = await accountService.ConfirmEmailAsync(userId, code);

            if (result.Succeeded) {
                TempData["SuccessMessage"] = result.SuccessMessage;
            }
            else {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Login));
        }

        ///////////////////////// FORGOT PASSWORD /////////////////////////
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

            await accountService.RequestPasswordResetAsync(model, (email, code) =>
                Url.Action(nameof(ResetPassword), "Account", new { email, code }, Request.Scheme));

            TempData["SuccessMessage"] = "A password reset link has been sent.";
            return RedirectToAction(nameof(Login));
        }

        ///////////////////////// RESET PASSWORD /////////////////////////
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

            var result = await accountService.ResetPasswordAsync(model);

            if (result.Succeeded) {
                TempData["SuccessMessage"] = result.SuccessMessage;
                return RedirectToAction(nameof(Login));
            }

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage)) {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }

            AddIdentityErrors(result);
            return View(model);
        }

        ///////////////////////// LOGOUT /////////////////////////
        [HttpGet]
        public async Task<IActionResult> Logout() {
            await accountService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        private void AddIdentityErrors(AccountOperationResult result) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
