using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.Auth;

public record ConfirmEmailRequest(
    [Required] string UserId,
    [Required] string Token
);
