using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Application.Common.Interfaces.Authentication;

public record AuthenticationResult(
    User User,
    string Token
);