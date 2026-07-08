using System.Net.Http;
using System.Net.Http.Json;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using WestcoastCars.Contracts.Admin;
using WestcoastCars.Contracts.Auth;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Contracts.Verification;
using Xunit;

namespace WestcoastCars.Api.IntegrationTests;

public class IntegrationTestBase(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private static AuthenticationResponse? _cachedAdminAuthResponse;
    protected readonly CustomWebApplicationFactory<Program> _factory = factory;
    protected readonly HttpClient _client = factory.CreateClient();

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

    /// <summary>
    /// Registers a new customer, confirms the email via the link captured by TestEmailService,
    /// and returns the resulting JWT — mirrors the real register -> click link -> logged in flow.
    /// </summary>
    protected async Task<AuthenticationResponse> RegisterAndConfirmAsync(string firstName, string lastName, string email, string password)
    {
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(firstName, lastName, email, password));
        registerResponse.EnsureSuccessStatusCode();

        var confirmationLink = CustomWebApplicationFactory<Program>.GetLastConfirmationLink(email);
        Assert.NotNull(confirmationLink);

        var confirmResponse = await _client.GetAsync(confirmationLink);
        confirmResponse.EnsureSuccessStatusCode();
        var authResponse = await confirmResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(authResponse);
        return authResponse;
    }

    /// <summary>
    /// Runs the guest OTP flow (request-code -> read the code TestEmailService captured -> confirm-code)
    /// and returns a verified-email token to submit with an anonymous service booking.
    /// </summary>
    protected async Task<string> GetVerifiedEmailTokenAsync(string email)
    {
        var requestResponse = await _client.PostAsJsonAsync(
            "/api/v1/service-bookings/verification/request-code",
            new RequestVerificationCodeDto { Email = email });
        requestResponse.EnsureSuccessStatusCode();
        var requestBody = await requestResponse.Content.ReadFromJsonAsync<RequestVerificationCodeResponseDto>();
        Assert.NotNull(requestBody);

        var code = CustomWebApplicationFactory<Program>.GetLastVerificationCode(email);
        Assert.NotNull(code);

        var confirmResponse = await _client.PostAsJsonAsync(
            "/api/v1/service-bookings/verification/confirm-code",
            new ConfirmVerificationCodeDto { SessionToken = requestBody.SessionToken, Code = code });
        confirmResponse.EnsureSuccessStatusCode();
        var confirmBody = await confirmResponse.Content.ReadFromJsonAsync<ConfirmVerificationCodeResponseDto>();
        Assert.NotNull(confirmBody);

        return confirmBody.VerifiedEmailToken;
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
