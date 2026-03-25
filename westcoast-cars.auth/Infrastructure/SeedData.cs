using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WestcoastCars.Auth.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // Added this

namespace WestcoastCars.Auth.Infrastructure
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminUser(AuthDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, string defaultPassword, ILogger logger)
        {
            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                logger.LogInformation("Created 'Admin' role.");
            }
            if (!await roleManager.RoleExistsAsync("Salesperson"))
            {
                await roleManager.CreateAsync(new IdentityRole("Salesperson"));
                logger.LogInformation("Created 'Salesperson' role.");
            }
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
                logger.LogInformation("Created 'Customer' role.");
            }

            if (string.IsNullOrEmpty(defaultPassword))
            {
                logger.LogError("Default password is null or empty. Cannot seed users.");
                return;
            }

            // Seed Admin User
            if (await userManager.FindByNameAsync("admin@westcoast-cars.com") == null)
            {
                logger.LogInformation("Admin user not found, attempting to create.");
                var adminUser = new IdentityUser
                {
                    UserName = "admin@westcoast-cars.com",
                    Email = "admin@westcoast-cars.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, defaultPassword);
                if (result.Succeeded)
                {
                    logger.LogInformation("userManager.CreateAsync succeeded for admin user.");
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddClaimsAsync(adminUser, new[]
                    {
                        new System.Security.Claims.Claim("firstName", "Admin"),
                        new System.Security.Claims.Claim("lastName", "User")
                    });
                    logger.LogInformation("Admin user created and assigned to Admin role.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create admin user. Errors: {Errors}", errors);
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists. No action taken.");
            }

            // Seed Standard Customer User
            if (await userManager.FindByNameAsync("user@westcoast-cars.com") == null)
            {
                logger.LogInformation("Customer user not found, attempting to create.");
                var customerUser = new IdentityUser
                {
                    UserName = "user@westcoast-cars.com",
                    Email = "user@westcoast-cars.com",
                    EmailConfirmed = true
                };

                // Using default password for simplicity in dev/test
                var result = await userManager.CreateAsync(customerUser, defaultPassword);
                if (result.Succeeded)
                {
                    logger.LogInformation("Customer user created.");
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                    await userManager.AddClaimsAsync(customerUser, new[]
                    {
                        new Claim("firstName", "Test"),
                        new Claim("lastName", "User")
                    });
                    logger.LogInformation("Customer user assigned to Customer role.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create customer user. Errors: {Errors}", errors);
                }
            }
        }
    }
}
