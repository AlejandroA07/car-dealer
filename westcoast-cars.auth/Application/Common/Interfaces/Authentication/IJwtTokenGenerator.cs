using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Application.Common.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}