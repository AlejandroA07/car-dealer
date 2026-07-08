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
        var customerEmail = $"integration-{Guid.NewGuid():N}@example.com";
        var verifiedEmailToken = await GetVerifiedEmailTokenAsync(customerEmail);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = registrationNumber,
            ServiceType = "Annual service",
            BookingDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = (int)TimeSlot.Morning,
            CustomerName = "Integration Customer",
            CustomerEmail = customerEmail,
            CustomerPhone = "0700000000",
            Description = "Booking flow integration test",
            VerifiedEmailToken = verifiedEmailToken
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
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
        var customerEmail = $"missing-{Guid.NewGuid():N}@example.com";
        var verifiedEmailToken = await GetVerifiedEmailTokenAsync(customerEmail);

        var response = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "UNKNOWN1",
            ServiceType = "Brake service",
            BookingDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = (int)TimeSlot.Afternoon,
            CustomerName = "Missing Vehicle",
            CustomerEmail = customerEmail,
            CustomerPhone = "0700000001",
            Description = "Invalid registration",
            VerifiedEmailToken = verifiedEmailToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
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
        var firstEmail = $"first-{Guid.NewGuid():N}@example.com";
        var secondEmail = $"second-{Guid.NewGuid():N}@example.com";
        var firstToken = await GetVerifiedEmailTokenAsync(firstEmail);
        var secondToken = await GetVerifiedEmailTokenAsync(secondEmail);

        var firstResponse = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "REGLOCK1",
            ServiceType = "Annual service",
            BookingDate = bookingDate,
            TimeSlot = (int)TimeSlot.Morning,
            CustomerName = "First Customer",
            CustomerEmail = firstEmail,
            CustomerPhone = "0700000002",
            VerifiedEmailToken = firstToken
        });

        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await _client.PostAsJsonAsync("/api/v1/service-bookings", new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "reglock1",
            ServiceType = "Brake service",
            BookingDate = bookingDate.AddDays(1),
            TimeSlot = (int)TimeSlot.Afternoon,
            CustomerName = "Second Customer",
            CustomerEmail = secondEmail,
            CustomerPhone = "0700000003",
            VerifiedEmailToken = secondToken
        });

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAvailability_ShouldReturnSlotsForNormalizedWeek()
    {
        // Use a Wednesday 2 weeks from today — handler normalizes to Monday
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dayOfWeek = (int)today.DayOfWeek;
        var daysToMonday = dayOfWeek == 0 ? -6 : 1 - dayOfWeek;
        var currentMonday = today.AddDays(daysToMonday);
        var targetMonday = currentMonday.AddDays(7);
        var weekMidpoint = targetMonday.AddDays(2); // Wednesday of that week, triggers normalization

        var response = await _client.GetAsync($"/api/v1/service-bookings/availability?weekStart={weekMidpoint:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var slots = await response.Content.ReadFromJsonAsync<List<SlotAvailabilityDto>>();
        slots.Should().NotBeNull();
        slots!.Count.Should().Be(15);
        slots.Min(x => x.Date).Should().Be(targetMonday);
    }

    [Fact]
    public async Task Create_ShouldAllowOnlyOneConcurrentBookingPerSlot()
    {
        var bookingDate = DateTime.UtcNow.AddDays(8);
        var firstEmail = $"first-slot-{Guid.NewGuid():N}@example.com";
        var secondEmail = $"second-slot-{Guid.NewGuid():N}@example.com";
        var firstToken = await GetVerifiedEmailTokenAsync(firstEmail);
        var secondToken = await GetVerifiedEmailTokenAsync(secondEmail);

        var firstRequest = new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "SLOT001",
            ServiceType = "Annual service",
            BookingDate = bookingDate,
            TimeSlot = (int)TimeSlot.MidMorning,
            CustomerName = "First Customer",
            CustomerEmail = firstEmail,
            CustomerPhone = "0700000010",
            VerifiedEmailToken = firstToken
        };

        var secondRequest = new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "SLOT002",
            ServiceType = "Annual service",
            BookingDate = bookingDate,
            TimeSlot = (int)TimeSlot.MidMorning,
            CustomerName = "Second Customer",
            CustomerEmail = secondEmail,
            CustomerPhone = "0700000011",
            VerifiedEmailToken = secondToken
        };

        var firstTask = _client.PostAsJsonAsync("/api/v1/service-bookings", firstRequest);
        var secondTask = _client.PostAsJsonAsync("/api/v1/service-bookings", secondRequest);

        var responses = await Task.WhenAll(firstTask, secondTask);

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).Should().Be(1);
    }
}
