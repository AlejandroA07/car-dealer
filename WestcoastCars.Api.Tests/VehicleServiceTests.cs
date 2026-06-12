using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using WestcoastCars.Web.Services;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class VehicleServiceTests
{
    [Fact]
    public async Task GetVehicleForCreateAsync_ShouldCacheDropdownReferenceData()
    {
        var handler = new StubHttpMessageHandler(ReferenceDataResponse);
        var service = CreateService(handler);

        var firstResult = await service.GetVehicleForCreateAsync();
        var secondResult = await service.GetVehicleForCreateAsync();

        Assert.Equal("Volvo", Assert.Single(firstResult.Manufacturers).Text);
        Assert.Equal("Diesel", Assert.Single(firstResult.FuelTypes).Text);
        Assert.Equal("Automatic", Assert.Single(firstResult.TransmissionTypes).Text);
        Assert.Equal("Volvo", Assert.Single(secondResult.Manufacturers).Text);
        Assert.Equal(1, handler.RequestCount("http://api.test/api/v1/manufacturers"));
        Assert.Equal(1, handler.RequestCount("http://api.test/api/v1/fueltypes"));
        Assert.Equal(1, handler.RequestCount("http://api.test/api/v1/transmissions"));
    }

    [Fact]
    public async Task GetVehicleForEditAsync_ShouldCacheDropdownReferenceData()
    {
        var handler = new StubHttpMessageHandler(ReferenceDataResponse);
        var service = CreateService(handler);

        var firstResult = await service.GetVehicleForEditAsync(42);
        var secondResult = await service.GetVehicleForEditAsync(42);

        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        Assert.Equal(1, firstResult.Vehicle.ManufacturerId);
        Assert.Equal(2, firstResult.Vehicle.FuelTypeId);
        Assert.Equal(3, firstResult.Vehicle.TransmissionTypeId);
        Assert.Equal(1, handler.RequestCount("http://api.test/api/v1/manufacturers"));
        Assert.Equal(1, handler.RequestCount("http://api.test/api/v1/fueltypes"));
        Assert.Equal(1, handler.RequestCount("http://api.test/api/v1/transmissions"));
        Assert.Equal(2, handler.RequestCount("http://api.test/api/v1/vehicles/42"));
    }

    [Fact]
    public async Task GetVehicleByIdAsync_ShouldDeserializeVehicleDetails_WhenApiSucceeds()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("""{"id":42,"registrationNumber":"ABC123"}""")
        });
        var service = CreateService(handler);

        var result = await service.GetVehicleByIdAsync(42);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("ABC123", result.RegistrationNumber);
        Assert.Equal(HttpMethod.Get, handler.RequestMethod);
        Assert.Equal("http://api.test/api/v1/vehicles/42", handler.RequestedUri?.ToString());
    }

    [Fact]
    public async Task DeleteVehicleAsync_ShouldSendDeleteRequestAndReturnTrue_WhenApiSucceeds()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var service = CreateService(handler);

        var result = await service.DeleteVehicleAsync(42);

        Assert.True(result);
        Assert.Equal(HttpMethod.Delete, handler.RequestMethod);
        Assert.Equal("http://api.test/api/v1/vehicles/42", handler.RequestedUri?.ToString());
    }

    [Fact]
    public async Task DeleteVehicleAsync_ShouldSendDeleteRequestAndReturnFalse_WhenApiFails()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var service = CreateService(handler);

        var result = await service.DeleteVehicleAsync(42);

        Assert.False(result);
        Assert.Equal(HttpMethod.Delete, handler.RequestMethod);
        Assert.Equal("http://api.test/api/v1/vehicles/42", handler.RequestedUri?.ToString());
    }

    private static VehicleService CreateService(StubHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var httpClientFactory = new StubHttpClientFactory(httpClient);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Services:ApiUrl"] = "http://api.test"
            })
            .Build();

        return new VehicleService(
            httpClientFactory,
            configuration,
            NullLogger<VehicleService>.Instance,
            new MemoryCache(new MemoryCacheOptions()));
    }

    private static HttpResponseMessage ReferenceDataResponse(HttpRequestMessage request)
    {
        return request.RequestUri?.AbsolutePath switch
        {
            "/api/v1/manufacturers" => OkJson("""[{"id":1,"name":"Volvo"}]"""),
            "/api/v1/fueltypes" => OkJson("""[{"id":2,"name":"Diesel"}]"""),
            "/api/v1/transmissions" => OkJson("""[{"id":3,"name":"Automatic"}]"""),
            "/api/v1/vehicles/42" => OkJson("""{"id":42,"registrationNumber":"ABC123","manufacturer":"Volvo","fuelType":"Diesel","transmissionType":"Automatic"}"""),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        };
    }

    private static HttpResponseMessage OkJson(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent(json)
        };
    }

    private static StringContent JsonContent(string json)
    {
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private sealed class StubHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        private readonly HttpClient _httpClient = httpClient;

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler = handler;
        private readonly Lock _requestLock = new();
        private readonly List<string> _requestedUris = [];

        public HttpMethod? RequestMethod { get; private set; }
        public Uri? RequestedUri { get; private set; }

        public int RequestCount(string uri)
        {
            lock (_requestLock)
            {
                return _requestedUris.Count(requestedUri => requestedUri == uri);
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (_requestLock)
            {
                RequestMethod = request.Method;
                RequestedUri = request.RequestUri;
                if (request.RequestUri is not null)
                {
                    _requestedUris.Add(request.RequestUri.ToString());
                }
            }

            return Task.FromResult(_handler(request));
        }
    }
}
