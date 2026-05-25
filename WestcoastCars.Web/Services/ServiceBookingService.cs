using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WestcoastCars.Web.ViewModels.ServiceBooking;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.Services;

public class ServiceBookingService : IServiceBookingService
{
    private const int AdminListPageSize = 200;

    private readonly HttpClient _httpClient;
    private readonly ILogger<ServiceBookingService> _logger;

    public ServiceBookingService(IHttpClientFactory httpClientFactory, ILogger<ServiceBookingService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _logger = logger;
    }

    public async Task<ServiceBookingActionResult> CreateBookingAsync(ServiceBookingViewModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/service-bookings", model);
            if (response.IsSuccessStatusCode)
            {
                var createdBooking = await response.Content.ReadFromJsonAsync<CreateServiceBookingResponseDto>();
                return ServiceBookingActionResult.Success(createdBooking?.Id);
            }

            return await CreateFailureResultAsync(response, "Det gick inte att skapa servicebokningen.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service booking");
            return ServiceBookingActionResult.Failure(null, "Det gick inte att kontakta bokningstjänsten.");
        }
    }

    public async Task<ServiceBookingDataResult<IReadOnlyList<ServiceBookingSummaryDto>>> ListActiveBookingsAsync()
    {
        return await ListBookingsAsync("active");
    }

    public async Task<ServiceBookingDataResult<IReadOnlyList<ServiceBookingSummaryDto>>> ListInactiveBookingsAsync()
    {
        return await ListBookingsAsync("inactive");
    }

    public async Task<ServiceBookingDataResult<IReadOnlyList<SlotAvailabilityDto>>> GetWeekSlotsAsync(DateOnly weekStart)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<SlotAvailabilityDto>>(
                $"api/v1/service-bookings/availability?weekStart={weekStart:yyyy-MM-dd}");
            return ServiceBookingDataResult<IReadOnlyList<SlotAvailabilityDto>>.Success((result ?? []).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching week slots for {WeekStart}", weekStart);
            return ServiceBookingDataResult<IReadOnlyList<SlotAvailabilityDto>>.Failure(
                null,
                "Det gick inte att hämta lediga tider just nu.",
                []);
        }
    }

    public async Task<ServiceBookingActionResult> CancelAsync(int id, string cancellationReason)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync(
                $"api/v1/service-bookings/{id}/cancel",
                new CancelServiceBookingDto { CancellationReason = cancellationReason });
            return response.IsSuccessStatusCode
                ? ServiceBookingActionResult.Success()
                : await CreateFailureResultAsync(response, "Det gick inte att avboka bokningen.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {Id}", id);
            return ServiceBookingActionResult.Failure(null, "Det gick inte att kontakta bokningstjänsten.");
        }
    }

    public async Task<ServiceBookingActionResult> CompleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.PatchAsync(
                $"api/v1/service-bookings/{id}/complete", null);
            return response.IsSuccessStatusCode
                ? ServiceBookingActionResult.Success()
                : await CreateFailureResultAsync(response, "Det gick inte att markera bokningen som klar.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing booking {Id}", id);
            return ServiceBookingActionResult.Failure(null, "Det gick inte att kontakta bokningstjänsten.");
        }
    }

    public async Task<ServiceBookingActionResult> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/service-bookings/{id}");
            return response.IsSuccessStatusCode
                ? ServiceBookingActionResult.Success()
                : await CreateFailureResultAsync(response, "Det gick inte att radera bokningen.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking {Id}", id);
            return ServiceBookingActionResult.Failure(null, "Det gick inte att kontakta bokningstjänsten.");
        }
    }

    private async Task<ServiceBookingDataResult<IReadOnlyList<ServiceBookingSummaryDto>>> ListBookingsAsync(string state)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<PagedResult<ServiceBookingSummaryDto>>(
                $"api/v1/service-bookings?state={state}&pageSize={AdminListPageSize}");
            return ServiceBookingDataResult<IReadOnlyList<ServiceBookingSummaryDto>>.Success(result?.Items ?? []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing {State} service bookings", state);
            return ServiceBookingDataResult<IReadOnlyList<ServiceBookingSummaryDto>>.Failure(
                null,
                "Det gick inte att hämta bokningarna just nu.",
                []);
        }
    }

    private static async Task<ServiceBookingActionResult> CreateFailureResultAsync(HttpResponseMessage response, string fallbackMessage)
    {
        ProblemDetails? problemDetails = null;

        try
        {
            problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        }
        catch
        {
            // Ignore payload parsing issues and fall back to a generic message.
        }

        return ServiceBookingActionResult.Failure(
            response.StatusCode,
            problemDetails?.Detail ?? fallbackMessage);
    }
}
