using Swashbuckle.AspNetCore.Filters;
using WestcoastCars.Contracts.Auth;

namespace WestcoastCars.Api.Swagger.Examples;

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest("user@westcoast-cars.com", "SecurePassword123!");
    }
}

