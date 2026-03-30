using System.Text;
using System.Text.Json;

namespace westcoast_cars.web.Services;

public class GenericDataService<TList, TPost> : IGenericDataService<TList, TPost>
    where TList : class
    where TPost : class
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _endpoint;
    private readonly JsonSerializerOptions _options;

    public GenericDataService(IHttpClientFactory httpClientFactory, ILogger logger, string endpoint)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _logger = logger;
        _endpoint = endpoint;
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<IList<TList>> ListAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/{_endpoint}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching {endpoint}: {StatusCode}", _endpoint, response.StatusCode);
                return new List<TList>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TList>>(json, _options) ?? new List<TList>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unavailable while fetching {endpoint}", _endpoint);
            return new List<TList>();
        }
    }

    public async Task<bool> CreateAsync(TPost model)
    {
        try
        {
            var jsonPayload = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"api/v1/{_endpoint}", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API Error when creating {endpoint}: {ErrorContent}", _endpoint, errorContent);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unavailable while creating {endpoint}", _endpoint);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/{_endpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unavailable while deleting {endpoint} with id {id}", _endpoint, id);
            return false;
        }
    }
}
