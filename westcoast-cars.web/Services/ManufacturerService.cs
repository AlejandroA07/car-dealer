using System.Text;
using System.Text.Json;
using westcoast_cars.web.ViewModels.Manufacturer;

namespace westcoast_cars.web.Services
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ManufacturerService> _logger;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _options;

        public ManufacturerService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ManufacturerService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _baseUrl = config["ApiBaseUrl"];
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IList<ManufacturerListViewModel>> ListAllManufacturersAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/manufacturers");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching manufacturers: {StatusCode}", response.StatusCode);
                return new List<ManufacturerListViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ManufacturerListViewModel>>(json, _options);
        }

        public async Task<bool> CreateManufacturerAsync(ManufacturerPostViewModel model)
        {
            var apiPayload = new { Name = model.Name };
            var jsonPayload = JsonSerializer.Serialize(apiPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/manufacturers", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API Error: {ErrorContent}", errorContent);
            return false;
        }

        public async Task<bool> DeleteManufacturerAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/v1/manufacturers/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
