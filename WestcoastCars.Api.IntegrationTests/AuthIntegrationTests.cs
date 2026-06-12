using System.Net;
using System.Net.Http.Json;
using WestcoastCars.Contracts.Admin;
using WestcoastCars.Contracts.Auth;

namespace WestcoastCars.Api.IntegrationTests;

public class AuthIntegrationTests(CustomWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_ShouldCreateCustomerAndReturnJwt()
    {
        var email = $"customer-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("Test", "Customer", email, "Password123!"));

        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();

        Assert.NotNull(authResponse);
        Assert.Equal(email, authResponse.Email);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.Token));
    }

    [Fact]
    public async Task Login_ShouldReturnJwtForSeededAdminUser()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin@westcoast-cars.com", "Password123!"));

        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();

        Assert.NotNull(authResponse);
        Assert.Equal("admin@westcoast-cars.com", authResponse.Email);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.Token));
    }

    [Fact]
    public async Task ProtectedVehicleEndpoint_ShouldRejectAnonymousUser()
    {
        var response = await _client.GetAsync("/api/v1/vehicles/list-all");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedVehicleEndpoint_ShouldAcceptAdminJwtFromRealLogin()
    {
        var authResponse = await LoginAsync("admin@westcoast-cars.com", "Password123!");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);

        var response = await client.GetAsync("/api/v1/vehicles/list-all");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Salesperson_ShouldAccessAllowedVehicleEndpoint()
    {
        var admin = await LoginAsync("admin@westcoast-cars.com", "Password123!");
        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", admin.Token);
        var email = $"sales-{Guid.NewGuid():N}@example.com";

        var createUserResponse = await adminClient.PostAsJsonAsync(
            "/api/admin/create-user",
            new CreateUserRequest("Sales", "User", email, "Password123!", "Salesperson"));
        createUserResponse.EnsureSuccessStatusCode();

        var salesperson = await LoginAsync(email, "Password123!");
        var salespersonClient = _factory.CreateClient();
        salespersonClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", salesperson.Token);

        var response = await salespersonClient.GetAsync("/api/v1/vehicles/list-all");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Customer_ShouldBeForbiddenFromAdminOnlyEndpoint()
    {
        var email = $"customer-{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("Test", "Customer", email, "Password123!"));
        registerResponse.EnsureSuccessStatusCode();
        var customer = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(customer);

        var customerClient = _factory.CreateClient();
        customerClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customer.Token);

        var response = await customerClient.DeleteAsync("/api/v1/vehicles/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin@westcoast-cars.com", "WrongPassword123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
