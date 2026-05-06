using System.Threading.Tasks;
using WestcoastCars.Web.ViewModels.Auth;

namespace WestcoastCars.Web.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
}
