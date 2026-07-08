using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using WestcoastCars.Contracts.Auth;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Contracts.Verification;
using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Api.IntegrationTests;

public class ServiceBookingVerificationIntegrationTests(CustomWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GuestFlow_RequestCode_ConfirmCode_ThenBook_ShouldSucceed()
    {
        var email = $"guest-{Guid.NewGuid():N}@example.com";
        var verifiedEmailToken = await GetVerifiedEmailTokenAsync(email);

        var response = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "GUEST001",
            ServiceType = "Annual service",
            BookingDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = (int)TimeSlot.Morning,
            CustomerName = "Guest Customer",
            CustomerEmail = email,
            CustomerPhone = "0700000099",
            VerifiedEmailToken = verifiedEmailToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ConfirmCode_ShouldReturnBadRequest_WhenCodeIsWrong()
    {
        var email = $"guest-{Guid.NewGuid():N}@example.com";
        var requestResponse = await _client.PostAsJsonAsync(
            "/api/v1/service-bookings/verification/request-code",
            new RequestVerificationCodeDto { Email = email });
        requestResponse.EnsureSuccessStatusCode();
        var requestBody = await requestResponse.Content.ReadFromJsonAsync<RequestVerificationCodeResponseDto>();

        var confirmResponse = await _client.PostAsJsonAsync(
            "/api/v1/service-bookings/verification/confirm-code",
            new ConfirmVerificationCodeDto { SessionToken = requestBody!.SessionToken, Code = "000000" });

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenVerifiedTokenEmailDoesNotMatchBookingEmail()
    {
        var verifiedEmail = $"verified-{Guid.NewGuid():N}@example.com";
        var verifiedEmailToken = await GetVerifiedEmailTokenAsync(verifiedEmail);

        var response = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "GUEST002",
            ServiceType = "Annual service",
            BookingDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = (int)TimeSlot.Afternoon,
            CustomerName = "Guest Customer",
            CustomerEmail = $"spoofed-{Guid.NewGuid():N}@example.com",
            CustomerPhone = "0700000098",
            VerifiedEmailToken = verifiedEmailToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenAnonymousAndNoVerifiedTokenProvided()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "GUEST003",
            ServiceType = "Annual service",
            BookingDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = (int)TimeSlot.MidMorning,
            CustomerName = "Guest Customer",
            CustomerEmail = $"unverified-{Guid.NewGuid():N}@example.com",
            CustomerPhone = "0700000097"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldSucceed_WhenAuthenticatedWithoutAVerifiedToken()
    {
        var email = $"customer-{Guid.NewGuid():N}@example.com";
        var customer = await RegisterAndConfirmAsync("Test", "Customer", email, "Password123!");
        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);

        var response = await authenticatedClient.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "GUEST004",
            ServiceType = "Annual service",
            BookingDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = (int)TimeSlot.Afternoon,
            CustomerName = "Test Customer",
            CustomerEmail = email,
            CustomerPhone = "0700000096"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task VerifiedEmailToken_ShouldNotAuthenticate_WhenUsedAsBearerToken()
    {
        var email = $"guest-{Guid.NewGuid():N}@example.com";
        var verifiedEmailToken = await GetVerifiedEmailTokenAsync(email);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", verifiedEmailToken);

        var response = await client.GetAsync("/api/v1/vehicles/list-all");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
