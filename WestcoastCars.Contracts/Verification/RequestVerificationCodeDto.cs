using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.Verification;

public class RequestVerificationCodeDto
{
    [Required(ErrorMessage = "E-post måste anges")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}
