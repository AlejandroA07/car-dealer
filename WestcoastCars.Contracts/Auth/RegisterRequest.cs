using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.Auth;

public record RegisterRequest(
    [Required, MaxLength(50)] string FirstName,
    [Required, MaxLength(50)] string LastName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password
);
