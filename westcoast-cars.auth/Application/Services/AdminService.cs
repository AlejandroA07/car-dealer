using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WestcoastCars.Auth.Application.Common.Interfaces.Authentication;
using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IJwtTokenGenerator jwtTokenGenerator,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminService> logger)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<AuthenticationResult> CreateUserAsync(string firstName, string lastName, string email, string password, string role)
        {
            if (await _userManager.FindByEmailAsync(email) is not null)
            {
                throw new Exception("User with given email already exists");
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                throw new Exception($"Role {role} does not exist");
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

            await _userManager.AddToRoleAsync(user, role);

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
}
