using WestcoastCars.Auth.Application.Common.Interfaces.Authentication;
using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Application.Services;

public interface IAuthService
{
    Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<AuthenticationResult?> LoginAsync(string email, string password);
}
