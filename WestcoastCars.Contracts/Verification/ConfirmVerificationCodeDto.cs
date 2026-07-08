using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.Verification;

public class ConfirmVerificationCodeDto
{
    [Required(ErrorMessage = "Sessionstoken måste anges")]
    public string SessionToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kod måste anges")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Koden måste bestå av 6 siffror")]
    public string Code { get; set; } = string.Empty;
}
