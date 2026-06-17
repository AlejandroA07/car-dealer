using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace WestcoastCars.Infrastructure.Data;

public static class IdentitySeedData
{
    private record SeedUser(string Email, string Role, string FirstName, string LastName);

    private static readonly SeedUser[] Users =
    [
        new("admin@westcoast-cars.com",       "Admin",       "Admin",       "One"),
        new("admin2@westcoast-cars.com",      "Admin",       "Admin",       "Two"),
        new("salesperson@westcoast-cars.com", "Salesperson", "Sales",       "One"),
        new("salesperson2@westcoast-cars.com","Salesperson", "Sales",       "Two"),
        new("user@westcoast-cars.com",        "Customer",    "Test",        "User"),
        new("user2@westcoast-cars.com",       "Customer",    "Test",        "Two"),
    ];

    public static async Task SeedRolesAndUsers(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, string defaultPassword, ILogger logger)
    {
        foreach (var role in new[] { "Admin", "Salesperson", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created '{Role}' role.", role);
            }
        }

        foreach (var u in Users)
        {
            if (await userManager.FindByNameAsync(u.Email) != null)
            {
                logger.LogInformation("User {Email} already exists, skipping.", u.Email);
                continue;
            }

            var identityUser = new IdentityUser
            {
                UserName = u.Email,
                Email = u.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(identityUser, defaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(identityUser, u.Role);
                await userManager.AddClaimsAsync(identityUser,
                [
                    new Claim("firstName", u.FirstName),
                    new Claim("lastName",  u.LastName)
                ]);
                logger.LogInformation("Created user {Email} with role {Role}.", u.Email, u.Role);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create user {Email}. Errors: {Errors}", u.Email, errors);
            }
        }
    }
}
