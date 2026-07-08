using System.Collections.Concurrent;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using WestcoastCars.Application.Services;
using WestcoastCars.Domain.Common.Enums;
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
        builder.UseSetting("RateLimiting:BookingCreatePermitLimit", "100");
        builder.UseSetting("RateLimiting:AuthPermitLimit", "1000");
        builder.UseSetting("RateLimiting:OtpRequestPermitLimit", "100");
        builder.UseSetting("RateLimiting:OtpConfirmPermitLimit", "100");
        builder.UseSetting("JwtSettings:Secret", "super-secret-key-for-testing-purposes-only-123");
        builder.UseSetting("JwtSettings:Issuer", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:Audience", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "60");
        builder.UseSetting("App:BaseUrl", "");
        builder.UseSetting("GuestVerification:Secret", "another-super-secret-key-for-testing-purposes-only-456");
        builder.UseSetting("GuestVerification:Issuer", "WestcoastCars.GuestVerification");
        builder.UseSetting("GuestVerification:Audience", "WestcoastCars.GuestVerification");
        builder.UseSetting("GuestVerification:CodeLength", "6");
        builder.UseSetting("GuestVerification:CodeExpiryMinutes", "10");
        builder.UseSetting("GuestVerification:VerifiedTokenExpiryMinutes", "20");
        builder.UseSetting("AdminSettings:Password", AdminPassword);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, TestEmailService>();
        });
    }

    public static string? GetLastConfirmationLink(string email) => TestEmailService.LastConfirmationLinks.GetValueOrDefault(NormalizeEmail(email));

    public static string? GetLastVerificationCode(string email) => TestEmailService.LastVerificationCodes.GetValueOrDefault(NormalizeEmail(email));

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public async Task ResetDatabaseAsync()
    {
        TestEmailService.LastConfirmationLinks.Clear();
        TestEmailService.LastVerificationCodes.Clear();

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

        await SeedData.LoadVehicleData(context);

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

    private sealed class TestEmailService : IEmailService
    {
        public static readonly ConcurrentDictionary<string, string> LastConfirmationLinks = new();
        public static readonly ConcurrentDictionary<string, string> LastVerificationCodes = new();

        public Task SendEmailVerificationAsync(string toEmail, string name, string confirmationLink)
        {
            LastConfirmationLinks[NormalizeEmail(toEmail)] = confirmationLink;
            return Task.CompletedTask;
        }

        public Task SendVerificationCodeAsync(string toEmail, string code, int expiryMinutes)
        {
            LastVerificationCodes[NormalizeEmail(toEmail)] = code;
            return Task.CompletedTask;
        }

        public Task SendBookingConfirmationAsync(
            string toEmail,
            string customerName,
            DateTime bookingDate,
            TimeSlot timeSlot,
            string serviceType,
            string vehicleRegistrationNumber)
        {
            return Task.CompletedTask;
        }

        public Task SendCancellationNoticeAsync(
            string toEmail,
            string customerName,
            DateTime bookingDate,
            TimeSlot timeSlot,
            string reason)
        {
            return Task.CompletedTask;
        }
    }
}
