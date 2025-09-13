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
        
        return services;
    }
}
