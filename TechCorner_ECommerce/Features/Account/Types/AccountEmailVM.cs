using System.ComponentModel.DataAnnotations;

namespace TechCorner_ECommerce.ViewModels {
    public class ForgotPasswordVM {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordVM {
        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
