using WestcoastCars.Application.Models.Authentication;

namespace WestcoastCars.Application.Common.Interfaces.Authentication;

public record AuthenticationResult(
    AuthenticatedUser User,
    string Token
);
