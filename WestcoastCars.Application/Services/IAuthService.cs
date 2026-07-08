using WestcoastCars.Application.Common.Interfaces.Authentication;

namespace WestcoastCars.Application.Services;

public interface IAuthService
{
    Task RegisterAsync(string firstName, string lastName, string email, string password, string confirmationLinkBase);
    Task<AuthenticationResult?> LoginAsync(string email, string password);
    Task<AuthenticationResult> ConfirmEmailAsync(string userId, string token);
}
