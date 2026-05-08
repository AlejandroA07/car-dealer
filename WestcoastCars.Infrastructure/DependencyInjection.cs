using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WestcoastCars.Application.Common.Interfaces.Authentication;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Clients;
using WestcoastCars.Infrastructure.Authentication;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Options;
using WestcoastCars.Infrastructure.Repositories;
using WestcoastCars.Infrastructure.Services;
using System;

namespace WestcoastCars.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BlocketApiOptions>(configuration.GetSection(BlocketApiOptions.SectionName));
        services.AddHttpClient<IBlocketApiClient, BlocketApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<BlocketApiOptions>>()
                .Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WestcoastCars/1.0");
        });

        var connectionString = ResolvePostgreSqlConnectionString(configuration);

        if (IsSqliteInMemory(connectionString))
        {
            services.AddDbContext<WestcoastCarsContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            services.AddDbContext<WestcoastCarsContext>(options =>
                options.UseNpgsql(connectionString, postgresOptions =>
                {
                    postgresOptions.MigrationsHistoryTable("__EFMigrationsHistory_WestcoastCars");
                    postgresOptions.EnableRetryOnFailure();
                }));
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        services.AddScoped<IFuelTypeRepository, FuelTypeRepository>();
        services.AddScoped<ITransmissionTypeRepository, TransmissionTypeRepository>();
        services.AddScoped<IServiceBookingRepository, ServiceBookingRepository>();

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<WestcoastCarsContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<WestcoastCars.Application.Services.IAuthService, AuthService>();
        services.AddScoped<WestcoastCars.Application.Services.IAdminService, AdminService>();

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

        var composePassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (connectionString is not null && composePassword is not null)
        {
            connectionString = connectionString.Replace("${POSTGRES_PASSWORD}", composePassword);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing. Set ConnectionStrings__DefaultConnection.");
        }

        return connectionString;
    }
}
