namespace WestcoastCars.Application.Models.Authentication;

public record AuthenticatedUser(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
);
