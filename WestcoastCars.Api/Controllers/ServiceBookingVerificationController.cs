using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WestcoastCars.Application.Services;
using WestcoastCars.Contracts.Verification;

namespace WestcoastCars.Api.Controllers;

/// <summary>
/// Handles email verification for guests booking a service without an account.
/// </summary>
[ApiController]
[Route("api/v1/service-bookings/verification")]
[Tags("Service Bookings")]
[AllowAnonymous]
public class ServiceBookingVerificationController(IGuestEmailVerificationService verificationService) : ControllerBase
{
    private readonly IGuestEmailVerificationService _verificationService = verificationService;

    /// <summary>
    /// Sends a one-time verification code to the given email address. Calling this again resends a fresh code.
    /// </summary>
    /// <response code="202">A verification code was sent (if applicable).</response>
    /// <response code="400">Invalid email address.</response>
    [HttpPost("request-code")]
    [EnableRateLimiting("otp-request")]
    [ProducesResponseType(typeof(RequestVerificationCodeResponseDto), 202)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RequestCode(RequestVerificationCodeDto dto)
    {
        var sessionToken = await _verificationService.RequestCodeAsync(dto.Email);
        return Accepted(new RequestVerificationCodeResponseDto { SessionToken = sessionToken });
    }

    /// <summary>
    /// Confirms a one-time verification code and returns a short-lived token proving the email was verified.
    /// </summary>
    /// <response code="200">Code confirmed; returns a verified-email token to submit with the booking.</response>
    /// <response code="400">Invalid, expired, or incorrect code.</response>
    [HttpPost("confirm-code")]
    [EnableRateLimiting("otp-confirm")]
    [ProducesResponseType(typeof(ConfirmVerificationCodeResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConfirmCode(ConfirmVerificationCodeDto dto)
    {
        var verifiedToken = await _verificationService.ConfirmCodeAsync(dto.SessionToken, dto.Code);
        return Ok(new ConfirmVerificationCodeResponseDto { VerifiedEmailToken = verifiedToken });
    }
}
