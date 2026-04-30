using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Auth.Infrastructure.Data;

using WestcoastCars.Auth.Application.Common.Interfaces.Authentication;
using WestcoastCars.Auth.Application.Common.Interfaces.Services;
using WestcoastCars.Auth.Infrastructure.Authentication;
using WestcoastCars.Auth.Infrastructure.Services;
using WestcoastCars.Auth.Application.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;

namespace WestcoastCars.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext<AuthDbContext>(options =>
        {
            var connectionString = ResolvePostgreSqlConnectionString(configuration);

            if (IsSqliteInMemory(connectionString))
            {
                options.UseSqlite(connectionString);
                return;
            }

            options.UseNpgsql(connectionString, postgresOptions =>
            {
                postgresOptions.MigrationsHistoryTable("__EFMigrationsHistory_Auth");
                postgresOptions.EnableRetryOnFailure();
            });
        });

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false; // Simpler passwords for demo
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }

    private static bool IsSqliteInMemory(string connectionString)
    {
        return connectionString == "DataSource=:memory:" ||
            connectionString.Contains("Mode=Memory", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolvePostgreSqlConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var host = Environment.GetEnvironmentVariable("PGHOST");
            var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
            var database = Environment.GetEnvironmentVariable("PGDATABASE");
            var user = Environment.GetEnvironmentVariable("PGUSER");
            var password = Environment.GetEnvironmentVariable("PGPASSWORD");

            if (!string.IsNullOrWhiteSpace(host) &&
                !string.IsNullOrWhiteSpace(database) &&
                !string.IsNullOrWhiteSpace(user) &&
                !string.IsNullOrWhiteSpace(password))
            {
                connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};";
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var postgresUrl = Environment.GetEnvironmentVariable("POSTGRES_URL") ??
                Environment.GetEnvironmentVariable("DATABASE_URL");

            if (!string.IsNullOrWhiteSpace(postgresUrl) &&
                (postgresUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                 postgresUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
            {
                var uri = new Uri(postgresUrl);
                var userInfo = uri.UserInfo.Split(':', 2);
                var uriUser = Uri.UnescapeDataString(userInfo[0]);
                var uriPassword = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
                var uriDatabase = uri.AbsolutePath.Trim('/');
                connectionString = $"Host={uri.Host};Port={uri.Port};Database={uriDatabase};Username={uriUser};Password={uriPassword};";
            }
            else if (!string.IsNullOrWhiteSpace(postgresUrl))
            {
                connectionString = postgresUrl;
            }
        }

        var composePassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (connectionString is not null && composePassword is not null)
        {
            connectionString = connectionString.Replace("${POSTGRES_PASSWORD}", composePassword);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing. Set ConnectionStrings__DefaultConnection, POSTGRES_URL, DATABASE_URL, or PGHOST/PGPORT/PGDATABASE/PGUSER/PGPASSWORD.");
        }

        return connectionString;
    }
}
