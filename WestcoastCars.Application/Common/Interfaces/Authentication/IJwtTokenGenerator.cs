using WestcoastCars.Application.Models.Authentication;

namespace WestcoastCars.Application.Common.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(AuthenticatedUser user, IEnumerable<string> roles);
}
