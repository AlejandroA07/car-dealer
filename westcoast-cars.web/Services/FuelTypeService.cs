using System.Text;
using System.Text.Json;
using westcoast_cars.web.ViewModels.FuelType;

namespace westcoast_cars.web.Services
{
    public class FuelTypeService : IFuelTypeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FuelTypeService> _logger;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _options;

        public FuelTypeService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<FuelTypeService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _baseUrl = config["ApiBaseUrl"];
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IList<FuelTypeListViewModel>> ListAllFuelTypesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/fueltypes");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching fuel types: {StatusCode}", response.StatusCode);
                return new List<FuelTypeListViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<FuelTypeListViewModel>>(json, _options);
        }

        public async Task<bool> CreateFuelTypeAsync(FuelTypePostViewModel model)
        {
            var apiPayload = new { Name = model.Name };
            var jsonPayload = JsonSerializer.Serialize(apiPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/fueltypes", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API Error: {ErrorContent}", errorContent);
            return false;
        }

        public async Task<bool> DeleteFuelTypeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/v1/fueltypes/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
