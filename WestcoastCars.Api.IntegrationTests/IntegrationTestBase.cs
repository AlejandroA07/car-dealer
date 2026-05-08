using System.Net.Http;
using System.Net.Http.Json;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using WestcoastCars.Contracts.Admin;
using WestcoastCars.Contracts.Auth;
using WestcoastCars.Contracts.DTOs;
using Xunit;

namespace WestcoastCars.Api.IntegrationTests;

public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private static AuthenticationResponse? _cachedAdminAuthResponse;
    protected readonly CustomWebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;

    public IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public virtual Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var authResponse = _cachedAdminAuthResponse ??= await LoginAsync("admin@westcoast-cars.com", "Password123!");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);
        return client;
    }

    protected async Task<HttpClient> CreateSalespersonClientAsync()
    {
        var adminClient = await CreateAuthenticatedClientAsync();
        var email = $"sales-{Guid.NewGuid():N}@example.com";

        var createUserResponse = await adminClient.PostAsJsonAsync(
            "/api/admin/create-user",
            new CreateUserRequest("Sales", "User", email, "Password123!", "Salesperson"));
        createUserResponse.EnsureSuccessStatusCode();

        var authResponse = await LoginAsync(email, "Password123!");
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

    protected async Task<(int ManufacturerId, int FuelTypeId, int TransmissionTypeId)> GetVehicleLookupIdsAsync()
    {
        var manufacturers = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/manufacturers");
        var fuelTypes = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/fueltypes");
        var transmissions = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/transmissions");

        Assert.NotNull(manufacturers);
        Assert.NotNull(fuelTypes);
        Assert.NotNull(transmissions);

        return (manufacturers!.First().Id, fuelTypes!.First().Id, transmissions!.First().Id);
    }

    protected async Task<VehicleDetailsDto> CreateVehicleAsync(HttpClient client, string registrationNumber, string model = "V60", bool isSold = false)
    {
        var (manufacturerId, fuelTypeId, transmissionTypeId) = await GetVehicleLookupIdsAsync();
        var command = new CreateVehicleCommand
        {
            RegistrationNumber = registrationNumber,
            ManufacturerId = manufacturerId,
            Model = model,
            ModelYear = 2024,
            Mileage = 100,
            FuelTypeId = fuelTypeId,
            TransmissionTypeId = transmissionTypeId,
            Price = 500000,
            Description = $"{model} integration test vehicle",
            IsSold = isSold
        };

        var response = await client.PostAsJsonAsync("/api/v1/vehicles", command);
        response.EnsureSuccessStatusCode();
        var vehicle = await response.Content.ReadFromJsonAsync<VehicleDetailsDto>();
        Assert.NotNull(vehicle);
        return vehicle!;
    }
}
