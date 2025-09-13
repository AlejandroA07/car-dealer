using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WestcoastCars.Auth.Infrastructure.Data;

public class AuthDbContext : IdentityDbContext<IdentityUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Roles
        const string ADMIN_ROLE_ID = "a1b1c1d1-e1f1-a1b1-c1d1-e1f1a1b1c1d1";
        const string SALESPERSON_ROLE_ID = "b2c2d2e2-f2a2-b2c2-d2e2-f2a2b2c2d2e2";
        const string CUSTOMER_ROLE_ID = "c3d3e3f3-a3b3-c3d3-e3f3-a3b3c3d3e3f3";

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = ADMIN_ROLE_ID,
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole
            {
                Id = SALESPERSON_ROLE_ID,
                Name = "Salesperson",
                NormalizedName = "SALESPERSON"
            },
            new IdentityRole
            {
                Id = CUSTOMER_ROLE_ID,
                Name = "Customer",
                NormalizedName = "CUSTOMER"
            }
        );

        // Seed Admin User
        const string ADMIN_USER_ID = "d4e4f4a4-b4c4-d4e4-f4a4-b4c4d4e4f4a4";
        var passwordHasher = new PasswordHasher<IdentityUser>();

        var adminUser = new IdentityUser
        {
            Id = ADMIN_USER_ID,
            UserName = "admin@westcoastcars.com",
            NormalizedUserName = "ADMIN@WESTCOASTCARS.COM",
            Email = "admin@westcoastcars.com",
            NormalizedEmail = "ADMIN@WESTCOASTCARS.COM",
            EmailConfirmed = true,
        };
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin123!");

        modelBuilder.Entity<IdentityUser>().HasData(adminUser);

        // Seed admin user claims
        modelBuilder.Entity<IdentityUserClaim<string>>().HasData(
            new IdentityUserClaim<string> { Id = 1, UserId = ADMIN_USER_ID, ClaimType = "firstName", ClaimValue = "Admin" },
            new IdentityUserClaim<string> { Id = 2, UserId = ADMIN_USER_ID, ClaimType = "lastName", ClaimValue = "User" }
        );

        // Assign Admin Role to Admin User
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = ADMIN_ROLE_ID,
                UserId = ADMIN_USER_ID
            }
        );
    }
}