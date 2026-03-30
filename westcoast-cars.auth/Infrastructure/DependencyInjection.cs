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
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;

namespace WestcoastCars.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext<AuthDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable("MYSQL_URL");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var host = Environment.GetEnvironmentVariable("MYSQLHOST");
                var port = Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306";
                var database = Environment.GetEnvironmentVariable("MYSQLDATABASE");
                var user = Environment.GetEnvironmentVariable("MYSQLUSER");
                var password = Environment.GetEnvironmentVariable("MYSQLPASSWORD");

                if (!string.IsNullOrWhiteSpace(host) &&
                    !string.IsNullOrWhiteSpace(database) &&
                    !string.IsNullOrWhiteSpace(user) &&
                    !string.IsNullOrWhiteSpace(password))
                {
                    connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
                }
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing. Set ConnectionStrings__DefaultConnection or MYSQL_URL (or MYSQLHOST/MYSQLPORT/MYSQLDATABASE/MYSQLUSER/MYSQLPASSWORD).");
            }

            if (environment.IsProduction())
            {
                var passwordFile = "/run/secrets/db_password";
                if (System.IO.File.Exists(passwordFile))
                {
                    var password = System.IO.File.ReadAllText(passwordFile).Trim();
                    var csBuilder = new System.Data.Common.DbConnectionStringBuilder
                    {
                        ConnectionString = connectionString
                    };
                    csBuilder["Password"] = password;
                    connectionString = csBuilder.ConnectionString;
                }
            }

            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
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
}
