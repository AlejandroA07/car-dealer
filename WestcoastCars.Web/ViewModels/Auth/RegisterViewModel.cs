using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Web.ViewModels.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Förnamn måste anges.")]
    [MaxLength(50)]
    [Display(Name = "Förnamn")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Efternamn måste anges.")]
    [MaxLength(50)]
    [Display(Name = "Efternamn")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-post måste anges.")]
    [EmailAddress(ErrorMessage = "Ogiltigt e-postformat.")]
    [MaxLength(256)]
    [Display(Name = "E-post")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lösenord måste anges.")]
    [MinLength(8, ErrorMessage = "Lösenordet måste vara minst 8 tecken.")]
    [MaxLength(100)]
    [DataType(DataType.Password)]
    [Display(Name = "Lösenord")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bekräfta lösenordet.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Lösenorden matchar inte.")]
    [Display(Name = "Bekräfta lösenord")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
