using System.Net.Http;
using System.Net.Http.Json;
using WestcoastCars.Contracts.Auth;
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

    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var authResponse = await LoginAsync("admin@westcoast-cars.com", "Password123!");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);
        return client;
    }

    protected async Task<AuthenticationResponse> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(authResponse);
        return authResponse;
    }
}
