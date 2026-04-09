using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Infrastructure.Options;

namespace WestcoastCars.Infrastructure.Clients;

public class BlocketApiClient : IBlocketApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly BlocketApiOptions _options;

    public BlocketApiClient(HttpClient httpClient, IOptions<BlocketApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<BlocketCarSearchResponse> SearchCarsAsync(BlocketCarSearchRequest request, CancellationToken cancellationToken = default)
    {
        var queryParameters = new Dictionary<string, string?>
        {
            ["page"] = request.Page.ToString(),
            ["sort_order"] = request.SortOrder ?? _options.DefaultSortOrder,
            ["org_id"] = request.OrgId,
            ["locations"] = request.Locations,
            ["models"] = request.Models,
            ["price_from"] = request.PriceFrom?.ToString(),
            ["price_to"] = request.PriceTo?.ToString(),
            ["year_from"] = request.YearFrom?.ToString(),
            ["year_to"] = request.YearTo?.ToString()
        };

        var url = QueryHelpers.AddQueryString("v1/search/car", queryParameters!);
        return await GetFromJsonAsync<BlocketCarSearchResponse>(url, cancellationToken);
    }

    public async Task<BlocketCarAdDetails> GetCarAdAsync(string id, CancellationToken cancellationToken = default)
    {
        var url = QueryHelpers.AddQueryString("v1/ad/car", new Dictionary<string, string?> { ["id"] = id });
        return await GetFromJsonAsync<BlocketCarAdDetails>(url, cancellationToken);
    }

    private async Task<T> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<T>(responseStream, JsonOptions, cancellationToken);

        if (payload is null)
        {
            throw new InvalidOperationException($"Blocket API returned an empty payload for '{requestUri}'.");
        }

        return payload;
    }
}
