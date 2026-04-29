using Swashbuckle.AspNetCore.Filters;
using WestcoastCars.Auth.Contracts.Auth;

namespace WestcoastCars.Api.Swagger.Examples;

public class AuthenticationResponseExample : IExamplesProvider<AuthenticationResponse>
{
    public AuthenticationResponse GetExamples()
    {
        return new AuthenticationResponse(
            Id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            FirstName: "John",
            LastName: "Doe",
            Email: "user@westcoast-cars.com",
            Token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        );
    }
}

