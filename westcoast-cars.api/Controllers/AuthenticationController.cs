using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Auth.Application.Services;
using WestcoastCars.Auth.Contracts.Auth;

namespace WestcoastCars.Api.Controllers;

/// <summary>
/// Handles user authentication and registration.
/// </summary>
[ApiController]
[Route("api/auth")]
[Tags("Authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthenticationController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <returns>Authentication token and user info.</returns>
    /// <response code="200">Registration successful.</response>
    /// <response code="400">Invalid registration data or email already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var authResult = await _authService.RegisterAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password
            );

            var response = new AuthenticationResponse(
                authResult.User.Id,
                authResult.User.FirstName,
                authResult.User.LastName,
                authResult.User.Email,
                authResult.Token
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>Authentication token and user info.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var authResult = await _authService.LoginAsync(
                request.Email,
                request.Password
            );

            if (authResult is null)
            {
                return Unauthorized("Invalid credentials");
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
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

