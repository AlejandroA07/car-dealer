using System.Text;
using System.Text.Json;

namespace westcoast_cars.web.Services;

public class GenericDataService<TList, TPost> : IGenericDataService<TList, TPost>
    where TList : class
    where TPost : class
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GenericDataService<TList, TPost>> _logger;
    private readonly string _baseUrl;
    private readonly string _endpoint;
    private readonly JsonSerializerOptions _options;

    public GenericDataService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger logger, string endpoint)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _logger = logger;
        _baseUrl = config["ApiBaseUrl"];
        _endpoint = endpoint;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<IList<TList>> ListAllAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/{_endpoint}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error fetching {endpoint}: {StatusCode}", _endpoint, response.StatusCode);
            return new List<TList>();
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<TList>>(json, _options);
    }

    public async Task<bool> CreateAsync(TPost model)
    {
        var jsonPayload = JsonSerializer.Serialize(model);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/{_endpoint}", content);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError("API Error when creating {endpoint}: {ErrorContent}", _endpoint, errorContent);
        return false;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/v1/{_endpoint}/{id}");
        return response.IsSuccessStatusCode;
    }
}