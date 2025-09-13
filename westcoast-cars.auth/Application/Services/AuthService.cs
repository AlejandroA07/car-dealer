using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using WestcoastCars.Auth.Application.Common.Interfaces.Authentication;
using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthService(
        IJwtTokenGenerator jwtTokenGenerator, 
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            throw new Exception("Invalid credentials");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            throw new Exception("Invalid credentials");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var firstName = claims.FirstOrDefault(c => c.Type == "firstName")?.Value ?? string.Empty;
        var lastName = claims.FirstOrDefault(c => c.Type == "lastName")?.Value ?? string.Empty;

        var domainUser = new User
        {
            Id = Guid.Parse(user.Id),
            FirstName = firstName,
            LastName = lastName,
            Email = user.Email!
        };

        var token = await _jwtTokenGenerator.GenerateTokenAsync(domainUser, roles);

        return new AuthenticationResult(domainUser, token);
    }

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new Exception("User with given email already exists");
        }

        var user = new IdentityUser
        {
            Email = email,
            UserName = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join("\n", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
        }

        await _userManager.AddClaimAsync(user, new Claim("firstName", firstName));
        await _userManager.AddClaimAsync(user, new Claim("lastName", lastName));

        await _userManager.AddToRoleAsync(user, "Customer");

        var roles = await _userManager.GetRolesAsync(user);

        var domainUser = new User
        {
            Id = Guid.Parse(user.Id),
            FirstName = firstName,
            LastName = lastName,
            Email = user.Email
        };

        var token = await _jwtTokenGenerator.GenerateTokenAsync(domainUser, roles);

        return new AuthenticationResult(domainUser, token);
    }
}
