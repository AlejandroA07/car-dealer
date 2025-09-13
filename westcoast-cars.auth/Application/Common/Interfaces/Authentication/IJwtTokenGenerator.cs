using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Application.Common.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(User user, IEnumerable<string> roles);
}