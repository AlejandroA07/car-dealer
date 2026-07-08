using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WestcoastCars.Web.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WestcoastCars.Contracts.Auth;

namespace WestcoastCars.Web.Services;

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

        try
        {
            var response = await _httpClient.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var authResponseJson = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthenticationResponse>(authResponseJson, _options);

                if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
                {
                    _logger.LogError("Auth API returned success but payload was empty or missing token.");
                    return LoginResult.Failure("Ogiltigt svar från inloggningstjänsten.");
                }

                _logger.LogInformation("Login successful for user {Email}", model.Email);
                return LoginResult.Success(authResponse.Token, authResponse.Email);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Login failed for user {Email}. Status: {StatusCode}, Error: {ErrorContent}", model.Email, response.StatusCode, errorContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return LoginResult.Failure("Bekräfta din e-post innan du loggar in.", response.StatusCode);
            }

            return LoginResult.Failure("Felaktig e-post eller lösenord.", response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Auth API is unavailable while logging in user {Email}.", model.Email);
            return LoginResult.Failure("Inloggningstjänsten är tillfälligt otillgänglig. Försök igen senare.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Auth API request timed out while logging in user {Email}.", model.Email);
            return LoginResult.Failure("Inloggningstjänsten svarar inte just nu. Försök igen senare.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Auth API returned an invalid JSON response while logging in user {Email}.", model.Email);
            return LoginResult.Failure("Ogiltigt svar från inloggningstjänsten.");
        }
    }

    public async Task<RegisterResult> RegisterAsync(RegisterViewModel model)
    {
        var registerRequest = new RegisterRequest(model.FirstName, model.LastName, model.Email, model.Password);
        var jsonPayload = JsonSerializer.Serialize(registerRequest, _options);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        _logger.LogInformation("Attempting to register user {Email}", model.Email);

        try
        {
            var response = await _httpClient.PostAsync("api/auth/register", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Registration accepted for user {Email}", model.Email);
                return RegisterResult.Success();
            }

            var fallback = response.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "E-postadressen är redan registrerad."
                : "Registreringen misslyckades. Kontrollera dina uppgifter.";

            return RegisterResult.Failure(response.StatusCode, await ExtractErrorAsync(response, fallback));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Auth API is unavailable while registering user {Email}.", model.Email);
            return RegisterResult.Failure(null, "Registreringstjänsten är tillfälligt otillgänglig. Försök igen senare.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Auth API request timed out while registering user {Email}.", model.Email);
            return RegisterResult.Failure(null, "Registreringstjänsten svarar inte just nu. Försök igen senare.");
        }
    }

    public async Task<LoginResult> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            var url = $"api/auth/confirm-email?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>(_options);
                if (authResponse is null || string.IsNullOrEmpty(authResponse.Token))
                {
                    _logger.LogError("Auth API returned success but payload was empty or missing token during email confirmation.");
                    return LoginResult.Failure("Ogiltigt svar från bekräftelsetjänsten.");
                }

                return LoginResult.Success(authResponse.Token, authResponse.Email);
            }

            return LoginResult.Failure(await ExtractErrorAsync(response, "Bekräftelselänken är ogiltig eller har gått ut."), response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Auth API is unavailable while confirming email for user {UserId}.", userId);
            return LoginResult.Failure("Tjänsten är tillfälligt otillgänglig. Försök igen senare.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Auth API request timed out while confirming email for user {UserId}.", userId);
            return LoginResult.Failure("Tjänsten svarar inte just nu. Försök igen senare.");
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

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response, string fallback)
    {
        try
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return problemDetails?.Detail ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
}
