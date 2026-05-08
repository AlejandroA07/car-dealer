using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Api.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    private const string AdminPassword = "Password123!";
    private PostgreSqlContainer? _dbContainer;
    private string _connectionString = string.Empty;

    async Task IAsyncLifetime.InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();

        await _dbContainer.StartAsync();
        _connectionString = _dbContainer.GetConnectionString();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.UseSetting("JwtSettings:Secret", "super-secret-key-for-testing-purposes-only-123");
        builder.UseSetting("JwtSettings:Issuer", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:Audience", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "60");
        builder.UseSetting("AdminSettings:Password", AdminPassword);
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();

        await context.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE
                "ServiceBookings",
                "Vehicles",
                "Manufacturers",
                "FuelTypes",
                "TransmissionTypes",
                "AspNetUserClaims",
                "AspNetUserLogins",
                "AspNetUserRoles",
                "AspNetUserTokens",
                "AspNetRoleClaims",
                "AspNetRoles",
                "AspNetUsers"
            RESTART IDENTITY CASCADE;
            """);

        var seedPresence = await SeedData.GetSeedPresenceAsync(context);
        await SeedData.LoadManufacturerData(context, seedPresence.HasManufacturers);
        await SeedData.LoadFuelTypeData(context, seedPresence.HasFuelTypes);
        await SeedData.LoadTransmissionsData(context, seedPresence.HasTransmissionTypes);
        await SeedData.LoadVehicleData(context, seedPresence.HasVehicles);
        await SeedData.EnsurePostgreSqlIdentitySequencesAsync(context);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CustomWebApplicationFactory<TProgram>>();

        await IdentitySeedData.SeedRolesAndUsers(userManager, roleManager, AdminPassword, logger);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_dbContainer is not null)
        {
            await _dbContainer.DisposeAsync();
        }
    }
}
