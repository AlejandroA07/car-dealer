using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Common.Interfaces.Authentication;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Models.Authentication;
using WestcoastCars.Application.Services;

namespace WestcoastCars.Infrastructure.Services;

public class AdminService(
    IJwtTokenGenerator jwtTokenGenerator,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<AdminService> logger) : IAdminService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly ILogger<AdminService> _logger = logger;

    public async Task<AuthenticationResult> CreateUserAsync(string firstName, string lastName, string email, string password, string role)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new ConflictException("User with given email already exists");
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            throw new ValidationException("Role", [$"Role {role} does not exist"]);
        }

        var user = new IdentityUser
        {
            Email = email,
            UserName = email,
            // Admin-created accounts are vouched for out-of-band; they don't go through
            // the customer email-confirmation flow, so mark the email confirmed up front
            // (otherwise RequireConfirmedEmail would lock these accounts out of login).
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new ValidationException("Identity", result.Errors.Select(e => e.Description));
        }

        await _userManager.AddClaimAsync(user, new Claim("firstName", firstName));
        await _userManager.AddClaimAsync(user, new Claim("lastName", lastName));

        await _userManager.AddToRoleAsync(user, role);

        var roles = await _userManager.GetRolesAsync(user);

        var authenticatedUser = new AuthenticatedUser(
            Guid.Parse(user.Id),
            firstName,
            lastName,
            user.Email!);

        var token = await _jwtTokenGenerator.GenerateTokenAsync(authenticatedUser, roles);

        return new AuthenticationResult(authenticatedUser, token);
    }
}
