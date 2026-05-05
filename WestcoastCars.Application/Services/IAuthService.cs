using WestcoastCars.Application.Common.Interfaces.Authentication;

namespace WestcoastCars.Application.Services;

public interface IAuthService
{
    Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<AuthenticationResult?> LoginAsync(string email, string password);
}
