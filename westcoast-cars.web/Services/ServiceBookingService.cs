using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using westcoast_cars.web.ViewModels.ServiceBooking;
using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.Services
{
    public class ServiceBookingService : IServiceBookingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceBookingService> _logger;

        public ServiceBookingService(IHttpClientFactory httpClientFactory, ILogger<ServiceBookingService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
        }

        public async Task<bool> CreateBookingAsync(ServiceBookingViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/service-bookings", model);
                return response.IsSuccessStatusCode;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating service booking");
                return false;
            }
        }

        public async Task<IEnumerable<ServiceBookingSummaryDto>> ListAllBookingsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<ServiceBookingSummaryDto>>("api/v1/service-bookings");
                return response ?? new List<ServiceBookingSummaryDto>();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error listing all service bookings");
                return new List<ServiceBookingSummaryDto>();
            }
        }
    }
}
