using System.Net.Http;
using Xunit;

namespace WestcoastCars.Api.IntegrationTests;

public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected readonly CustomWebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;

    public IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected HttpClient CreateAuthenticatedClient(string role = "Admin")
    {
        var client = _factory.CreateClient();
        var token = JwtTokenGenerator.GenerateToken("testuser", role);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
