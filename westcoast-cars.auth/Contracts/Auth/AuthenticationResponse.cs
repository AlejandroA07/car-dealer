namespace WestcoastCars.Auth.Contracts.Auth;

public record AuthenticationResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Token
);