using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using westcoast_cars.web.ViewModels.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WestcoastCars.Auth.Contracts.Auth; // Assuming this DTO exists in the auth contracts

namespace westcoast_cars.web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly string _authApiBaseUrl;
        private readonly JsonSerializerOptions _options;

        public AuthService(HttpClient httpClient, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _authApiBaseUrl = _configuration["AuthApiBaseUrl"]; // Get base URL from config
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<LoginResult> LoginAsync(LoginViewModel model)
        {
            var loginRequest = new LoginRequest(model.Email, model.Password); // Assuming LoginRequest DTO
            var jsonPayload = JsonSerializer.Serialize(loginRequest, _options);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _logger.LogInformation("Attempting to login user {Email} to Auth API at {Url}", model.Email, $"{_authApiBaseUrl}/auth/login");

            var response = await _httpClient.PostAsync($"{_authApiBaseUrl}/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var authResponseJson = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthenticationResponse>(authResponseJson, _options); // Assuming AuthenticationResponse DTO
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

        public async Task LogoutAsync()
        {
            // For now, just log out. The actual server-side logout (if any) would go here.
            _logger.LogInformation("User logged out from client-side.");
            await Task.CompletedTask; // Simulate async operation
        }
    }

    // Helper classes for LoginResult
    public class LoginResult
    {
        public bool IsSuccess { get; }
        public string Token { get; } // Store the JWT token
        public string Error { get; }

        private LoginResult(bool isSuccess, string token = null, string error = null)
        {
            IsSuccess = isSuccess;
            Token = token;
            Error = error;
        }

        public static LoginResult Success(string token) => new LoginResult(true, token);
        public static LoginResult Failure(string error) => new LoginResult(false, null, error);
    }
}
