using System.Threading.Tasks;
using westcoast_cars.web.ViewModels.Auth;

namespace westcoast_cars.web.Services
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
    }
}
