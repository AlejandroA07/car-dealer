using System.Text.Json;
using System.Net;
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

    // Process-wide by design: concurrent syncs share one Blocket request queue
    // so multiple admins cannot accidentally multiply outbound API traffic.
    private static readonly SemaphoreSlim RequestThrottle = new(1, 1);
    private static DateTimeOffset _nextAllowedRequestAt = DateTimeOffset.MinValue;

    private readonly HttpClient _httpClient;
    private readonly BlocketApiOptions _options;

    public BlocketApiClient(HttpClient httpClient, IOptions<BlocketApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options?.Value ?? new BlocketApiOptions();
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
        for (var attempt = 0; ; attempt++)
        {
            await WaitForRequestSlotAsync(cancellationToken);

            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (ShouldRetry(response.StatusCode) && attempt < _options.MaxRetries)
            {
                var retryDelay = GetRetryDelay(response, attempt);
                await Task.Delay(retryDelay, cancellationToken);
                continue;
            }

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

    private async Task WaitForRequestSlotAsync(CancellationToken cancellationToken)
    {
        await RequestThrottle.WaitAsync(cancellationToken);

        try
        {
            var now = DateTimeOffset.UtcNow;
            var delay = _nextAllowedRequestAt - now;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            var requestIntervalMilliseconds = Math.Max(0, _options.MinRequestIntervalMilliseconds);
            _nextAllowedRequestAt = DateTimeOffset.UtcNow.AddMilliseconds(requestIntervalMilliseconds);
        }
        finally
        {
            RequestThrottle.Release();
        }
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        var retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is not null)
        {
            return retryAfter.Delta.Value;
        }

        if (retryAfter?.Date is not null)
        {
            var untilRetry = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            if (untilRetry > TimeSpan.Zero)
            {
                return untilRetry;
            }
        }

        var seconds = Math.Min(8, Math.Pow(2, attempt + 1));
        return TimeSpan.FromSeconds(seconds);
    }
}
