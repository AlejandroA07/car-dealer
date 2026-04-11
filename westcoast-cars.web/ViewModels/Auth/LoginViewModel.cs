using System.ComponentModel.DataAnnotations;

namespace westcoast_cars.web.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-post måste anges.")]
        [EmailAddress(ErrorMessage = "Ogiltigt e-postformat.")]
        [Display(Name = "E-post")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lösenord måste anges.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lösenord")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Kom ihåg mig")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; } = "/";
    }
}
