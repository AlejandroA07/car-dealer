namespace WestcoastCars.Auth.Contracts.Auth;

public record AuthenticationResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Token
);