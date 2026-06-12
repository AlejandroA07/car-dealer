using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Api.IntegrationTests;

public class ServiceBookingsIntegrationTests(CustomWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
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
            BookingDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = (int)TimeSlot.Morning,
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
        var bookings = await listResponse.Content.ReadFromJsonAsync<PagedResult<ServiceBookingSummaryDto>>();
        bookings.Should().NotBeNull();
        bookings!.Items.Should().Contain(booking =>
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
            BookingDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = (int)TimeSlot.Afternoon,
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
        var bookings = await listResponse.Content.ReadFromJsonAsync<PagedResult<ServiceBookingSummaryDto>>();
        bookings.Should().NotBeNull();
        bookings!.Items.Should().Contain(booking =>
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
            TimeSlot = -1,
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

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenRegistrationAlreadyHasActiveBooking()
    {
        var bookingDate = DateTime.UtcNow.AddDays(7);

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "REGLOCK1",
            ServiceType = "Annual service",
            BookingDate = bookingDate,
            TimeSlot = (int)TimeSlot.Morning,
            CustomerName = "First Customer",
            CustomerEmail = "first@example.com",
            CustomerPhone = "0700000002"
        });

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "reglock1",
            ServiceType = "Brake service",
            BookingDate = bookingDate.AddDays(1),
            TimeSlot = (int)TimeSlot.Afternoon,
            CustomerName = "Second Customer",
            CustomerEmail = "second@example.com",
            CustomerPhone = "0700000003"
        });

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAvailability_ShouldReturnSlotsForNormalizedWeek()
    {
        var response = await _client.GetAsync("/api/v1/service-bookings/availability?weekStart=2026-05-27");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var slots = await response.Content.ReadFromJsonAsync<List<SlotAvailabilityDto>>();
        slots.Should().NotBeNull();
        slots!.Count.Should().Be(15);
        slots.Min(x => x.Date).Should().Be(new DateOnly(2026, 05, 25));
    }

    [Fact]
    public async Task Create_ShouldAllowOnlyOneConcurrentBookingPerSlot()
    {
        var bookingDate = DateTime.UtcNow.AddDays(8);

        var firstRequest = new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "SLOT001",
            ServiceType = "Annual service",
            BookingDate = bookingDate,
            TimeSlot = (int)TimeSlot.MidMorning,
            CustomerName = "First Customer",
            CustomerEmail = "first-slot@example.com",
            CustomerPhone = "0700000010"
        };

        var secondRequest = new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "SLOT002",
            ServiceType = "Annual service",
            BookingDate = bookingDate,
            TimeSlot = (int)TimeSlot.MidMorning,
            CustomerName = "Second Customer",
            CustomerEmail = "second-slot@example.com",
            CustomerPhone = "0700000011"
        };

        var firstTask = _client.PostAsJsonAsync("/api/v1/service-bookings", firstRequest);
        var secondTask = _client.PostAsJsonAsync("/api/v1/service-bookings", secondRequest);

        var responses = await Task.WhenAll(firstTask, secondTask);

        responses.Count(response => response.StatusCode == HttpStatusCode.OK).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).Should().Be(1);
    }
}
