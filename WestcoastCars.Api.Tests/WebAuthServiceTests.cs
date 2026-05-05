using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels.Auth;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class WebAuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldPostLoginToMainApiAuthEndpoint()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"id":"3fa85f64-5717-4562-b3fc-2c963f66afa6","firstName":"Admin","lastName":"User","email":"admin@westcoast-cars.com","token":"jwt-token"}""",
                Encoding.UTF8,
                "application/json")
        });
        var service = CreateService(handler);

        var result = await service.LoginAsync(new LoginViewModel { Email = "admin@westcoast-cars.com", Password = "Password123!" });

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("http://api.test/api/auth/login", handler.RequestedUri?.ToString());
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenMainApiRejectsLogin()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("Invalid credentials")
        });
        var service = CreateService(handler);

        var result = await service.LoginAsync(new LoginViewModel { Email = "admin@westcoast-cars.com", Password = "wrong" });

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenMainApiIsUnavailable()
    {
        var handler = new StubHttpMessageHandler(_ => throw new HttpRequestException("API unavailable"));
        var service = CreateService(handler);

        var result = await service.LoginAsync(new LoginViewModel { Email = "admin@westcoast-cars.com", Password = "Password123!" });

        Assert.False(result.IsSuccess);
    }

    private static AuthService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://api.test/")
        };

        return new AuthService(httpClient, NullLogger<AuthService>.Instance);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        public Uri? RequestedUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestedUri = request.RequestUri;
            return Task.FromResult(_handler(request));
        }
    }
}

