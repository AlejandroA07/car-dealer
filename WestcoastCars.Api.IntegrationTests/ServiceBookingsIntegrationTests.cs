using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Api.IntegrationTests;

public class ServiceBookingsIntegrationTests : IntegrationTestBase
{
    public ServiceBookingsIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAndList_ShouldPersistServiceBookingForExistingVehicle()
    {
        var adminClient = await CreateAuthenticatedClientAsync();
        var registrationNumber = $"BOOK{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        await CreateVehicleAsync(adminClient, registrationNumber, "Booking Model");

        var createResponse = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = registrationNumber,
            ServiceType = "Annual service",
            BookingDate = DateTime.SpecifyKind(new DateTime(2026, 5, 8), DateTimeKind.Utc),
            CustomerName = "Integration Customer",
            CustomerEmail = "integration@example.com",
            CustomerPhone = "0700000000",
            Description = "Booking flow integration test"
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdBooking = await createResponse.Content.ReadFromJsonAsync<CreateServiceBookingResponseDto>();
        createdBooking.Should().NotBeNull();
        createdBooking!.Id.Should().BePositive();

        var listResponse = await adminClient.GetAsync("/api/v1/service-bookings");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var bookings = await listResponse.Content.ReadFromJsonAsync<IEnumerable<ServiceBookingSummaryDto>>();
        bookings.Should().NotBeNull();
        bookings!.Should().Contain(booking =>
            booking.Id == createdBooking.Id &&
            booking.VehicleRegistrationNumber == registrationNumber &&
            booking.CustomerName == "Integration Customer");
    }

    [Fact]
    public async Task Create_ShouldPersistServiceBooking_WhenVehicleRegistrationDoesNotExist()
    {
        var adminClient = await CreateAuthenticatedClientAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "UNKNOWN1",
            ServiceType = "Brake service",
            BookingDate = DateTime.SpecifyKind(new DateTime(2026, 5, 8), DateTimeKind.Utc),
            CustomerName = "Missing Vehicle",
            CustomerEmail = "missing@example.com",
            CustomerPhone = "0700000001",
            Description = "Invalid registration"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdBooking = await response.Content.ReadFromJsonAsync<CreateServiceBookingResponseDto>();
        createdBooking.Should().NotBeNull();
        createdBooking!.Id.Should().BePositive();

        var listResponse = await adminClient.GetAsync("/api/v1/service-bookings");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var bookings = await listResponse.Content.ReadFromJsonAsync<IEnumerable<ServiceBookingSummaryDto>>();
        bookings.Should().NotBeNull();
        bookings!.Should().Contain(booking =>
            booking.Id == createdBooking.Id &&
            booking.VehicleRegistrationNumber == "UNKNOWN1" &&
            booking.CustomerName == "Missing Vehicle");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenPayloadIsInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = string.Empty,
            ServiceType = string.Empty,
            BookingDate = default,
            CustomerName = string.Empty,
            CustomerEmail = "not-an-email",
            CustomerPhone = string.Empty
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions.Should().ContainKey("traceId");
    }
}
