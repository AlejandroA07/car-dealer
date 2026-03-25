using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using westcoast_cars.web.ViewModels.Auth;
using Microsoft.Extensions.Logging;
using WestcoastCars.Auth.Contracts.Auth;

namespace westcoast_cars.web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;
        private readonly JsonSerializerOptions _options;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<LoginResult> LoginAsync(LoginViewModel model)
        {
            var loginRequest = new LoginRequest(model.Email, model.Password);
            var jsonPayload = JsonSerializer.Serialize(loginRequest, _options);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _logger.LogInformation("Attempting to login user {Email}", model.Email);

            var response = await _httpClient.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var authResponseJson = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthenticationResponse>(authResponseJson, _options);
                
                if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
                {
                    _logger.LogError("Auth API returned success but payload was empty or missing token.");
                    return LoginResult.Failure("Invalid response from auth service.");
                }

                _logger.LogInformation("Login successful for user {Email}", model.Email);
                return LoginResult.Success(authResponse.Token);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Login failed for user {Email}. Status: {StatusCode}, Error: {ErrorContent}", model.Email, response.StatusCode, errorContent);
                return LoginResult.Failure($"Login failed: {response.ReasonPhrase} - {errorContent}");
            }
        }

        /// <summary>
        /// Handles client-side logout operations (e.g., clearing cookies/tokens).
        /// Note: This does not invalidate the JWT on the server as JWTs are stateless.
        /// </summary>
        public Task LogoutAsync()
        {
            _logger.LogInformation("User logged out from client-side.");
            return Task.CompletedTask;
        }
    }
}
