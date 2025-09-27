using WestcoastCars.Auth.Application.Common.Interfaces.Authentication;

namespace WestcoastCars.Auth.Application.Services
{
    public interface IAdminService
    {
        Task<AuthenticationResult> CreateUserAsync(string firstName, string lastName, string email, string password, string role);
    }
}
