using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WestcoastCars.Auth.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WestcoastCars.Auth.Infrastructure
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminUser(AuthDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            var logger = userManager.Logger;

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

                var adminPassword = configuration["ADMIN_PASSWORD"];
                if (string.IsNullOrEmpty(adminPassword))
                {
                    logger.LogError("ADMIN_PASSWORD is not set in configuration. Cannot seed admin user.");
                    return;
                }

                var result = await userManager.CreateAsync(adminUser, adminPassword);
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
        }
    }
}
