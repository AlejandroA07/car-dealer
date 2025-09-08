using System.Text;
using System.Text.Json;
using westcoast_cars.web.ViewModels.TransmissionType;

namespace westcoast_cars.web.Services
{
    public class TransmissionTypeService : ITransmissionTypeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TransmissionTypeService> _logger;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _options;

        public TransmissionTypeService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<TransmissionTypeService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _baseUrl = config["ApiBaseUrl"];
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IList<TransmissionTypeListViewModel>> ListAllTransmissionTypesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/transmissions");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching transmission types: {StatusCode}", response.StatusCode);
                return new List<TransmissionTypeListViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TransmissionTypeListViewModel>>(json, _options);
        }

        public async Task<bool> CreateTransmissionTypeAsync(TransmissionTypePostViewModel model)
        {
            var apiPayload = new { Name = model.Name };
            var jsonPayload = JsonSerializer.Serialize(apiPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/transmissions", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API Error: {ErrorContent}", errorContent);
            return false;
        }

        public async Task<bool> DeleteTransmissionTypeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/v1/transmissions/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
