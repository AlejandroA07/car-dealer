using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Common.Interfaces.Authentication;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Models.Authentication;
using WestcoastCars.Application.Services;

namespace WestcoastCars.Infrastructure.Services;

public class AuthService(
    IJwtTokenGenerator jwtTokenGenerator,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IEmailService emailService,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly SignInManager<IdentityUser> _signInManager = signInManager;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<AuthenticationResult?> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found.");
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed for user {UserId}. IsLockedOut: {IsLockedOut}, IsNotAllowed: {IsNotAllowed}, RequiresTwoFactor: {RequiresTwoFactor}",
                user.Id, result.IsLockedOut, result.IsNotAllowed, result.RequiresTwoFactor);

            if (result.IsNotAllowed && !await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new EmailNotConfirmedException("Email address has not been confirmed.");
            }

            return null;
        }

        _logger.LogInformation("User {UserId} logged in successfully.", user.Id);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var firstName = claims.FirstOrDefault(c => c.Type == "firstName")?.Value ?? string.Empty;
        var lastName = claims.FirstOrDefault(c => c.Type == "lastName")?.Value ?? string.Empty;

        var authenticatedUser = new AuthenticatedUser(
            Guid.Parse(user.Id),
            firstName,
            lastName,
            user.Email!);

        var token = await _jwtTokenGenerator.GenerateTokenAsync(authenticatedUser, roles);

        return new AuthenticationResult(authenticatedUser, token);
    }

    public async Task RegisterAsync(string firstName, string lastName, string email, string password, string confirmationLinkBase)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new ConflictException("User with given email already exists");
        }

        var user = new IdentityUser
        {
            Email = email,
            UserName = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            _logger.LogWarning("User creation failed for {Email}: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Code)));
            throw new ValidationException("Registration",
                ["Registration failed. Ensure the email is valid and the password meets requirements."]);
        }

        await _userManager.AddClaimAsync(user, new Claim("firstName", firstName));
        await _userManager.AddClaimAsync(user, new Claim("lastName", lastName));

        await _userManager.AddToRoleAsync(user, "Customer");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{confirmationLinkBase}/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendEmailVerificationAsync(user.Email!, firstName, confirmationLink);
    }

    public async Task<AuthenticationResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new ValidationException(nameof(userId), ["The confirmation link is invalid or has expired."]);
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}",
                user.Id, string.Join(", ", result.Errors.Select(e => e.Code)));
            throw new ValidationException(nameof(token), ["The confirmation link is invalid or has expired."]);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var firstName = claims.FirstOrDefault(c => c.Type == "firstName")?.Value ?? string.Empty;
        var lastName = claims.FirstOrDefault(c => c.Type == "lastName")?.Value ?? string.Empty;

        var authenticatedUser = new AuthenticatedUser(
            Guid.Parse(user.Id),
            firstName,
            lastName,
            user.Email!);

        var jwtToken = await _jwtTokenGenerator.GenerateTokenAsync(authenticatedUser, roles);

        return new AuthenticationResult(authenticatedUser, jwtToken);
    }
}
