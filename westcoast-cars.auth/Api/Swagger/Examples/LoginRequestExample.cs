using Swashbuckle.AspNetCore.Filters;
using WestcoastCars.Auth.Contracts.Auth;

namespace WestcoastCars.Auth.Api.Swagger.Examples;

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest("user@westcoast-cars.com", "SecurePassword123!");
    }
}
