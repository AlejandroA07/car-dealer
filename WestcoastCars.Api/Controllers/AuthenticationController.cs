using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using WestcoastCars.Api.Configurations;
using WestcoastCars.Application.Common.Interfaces.Authentication;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Services;
using WestcoastCars.Contracts.Auth;

namespace WestcoastCars.Api.Controllers;

/// <summary>
/// Handles user authentication and registration.
/// </summary>
[ApiController]
[Route("api/auth")]
[Tags("Authentication")]
[EnableRateLimiting("auth")]
public class AuthenticationController(IAuthService authService, IOptions<AppOptions> appOptions) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly AppOptions _appOptions = appOptions.Value;

    /// <summary>
    /// Registers a new user account. A confirmation email is sent; the account cannot log in until confirmed.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <returns>An acknowledgement that a confirmation email was sent.</returns>
    /// <response code="202">Registration accepted, confirmation email sent.</response>
    /// <response code="400">Invalid registration data.</response>
    /// <response code="409">Email already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterPendingResponse), 202)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            await _authService.RegisterAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                _appOptions.BaseUrl
            );

            return Accepted(new RegisterPendingResponse("Check your email to confirm your account."));
        }
        catch (ConflictException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Registration failed.",
                Detail = "Registration failed. Check the provided details and try again.",
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Confirms a user's email address using the token from the confirmation email link.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="token">The email confirmation token.</param>
    /// <returns>Authentication token and user info.</returns>
    /// <response code="200">Email confirmed successfully.</response>
    /// <response code="400">Invalid or expired confirmation link.</response>
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var authResult = await _authService.ConfirmEmailAsync(userId, token);

        var response = new AuthenticationResponse(
            authResult.User.Id,
            authResult.User.FirstName,
            authResult.User.LastName,
            authResult.User.Email,
            authResult.Token
        );

        return Ok(response);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>Authentication token and user info.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="403">Email address has not been confirmed.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        AuthenticationResult? authResult;
        try
        {
            authResult = await _authService.LoginAsync(
                request.Email,
                request.Password
            );
        }
        catch (EmailNotConfirmedException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "Email not confirmed",
                Instance = HttpContext.Request.Path
            });
        }

        if (authResult is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid credentials",
                Instance = HttpContext.Request.Path
            });
        }

        var response = new AuthenticationResponse(
            authResult.User.Id,
            authResult.User.FirstName,
            authResult.User.LastName,
            authResult.User.Email,
            authResult.Token
        );

        return Ok(response);
    }
}
