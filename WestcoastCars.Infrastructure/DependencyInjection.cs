
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Repositories;
using WestcoastCars.Infrastructure.BackgroundJobs;
using System;

namespace WestcoastCars.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (connectionString == "DataSource=:memory:" || connectionString?.Contains("Mode=Memory") == true)
        {
            services.AddDbContext<WestcoastCarsContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            // Prefer Railway-provided MySQL environment variables when present.
            // This avoids relying on docker-compose style placeholders like ${MYSQL_PASSWORD} or service hosts like "db".
            var host = Environment.GetEnvironmentVariable("MYSQLHOST");
            var port = Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("MYSQLDATABASE");
            var user = Environment.GetEnvironmentVariable("MYSQLUSER");
            var mysqlPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD");

            if (!string.IsNullOrWhiteSpace(host) &&
                !string.IsNullOrWhiteSpace(database) &&
                !string.IsNullOrWhiteSpace(user) &&
                !string.IsNullOrWhiteSpace(mysqlPassword))
            {
                connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={mysqlPassword};";
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");

                if (!string.IsNullOrWhiteSpace(mysqlUrl) &&
                    mysqlUrl.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(mysqlUrl);
                    var userInfo = uri.UserInfo.Split(':', 2);
                    var uriUser = Uri.UnescapeDataString(userInfo[0]);
                    var uriPassword = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
                    var uriDatabase = uri.AbsolutePath.Trim('/'); // "/db" -> "db"
                    connectionString = $"Server={uri.Host};Port={uri.Port};Database={uriDatabase};Uid={uriUser};Pwd={uriPassword};";
                }
                else if (!string.IsNullOrWhiteSpace(mysqlUrl))
                {
                    connectionString = mysqlUrl;
                }
            }

            var composePassword = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");

            if (connectionString is not null && composePassword is not null)
            {
                connectionString = connectionString.Replace("${MYSQL_PASSWORD}", composePassword);
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing. Set ConnectionStrings__DefaultConnection or MYSQL_URL (or MYSQLHOST/MYSQLPORT/MYSQLDATABASE/MYSQLUSER/MYSQLPASSWORD).");
            }

            services.AddDbContext<WestcoastCarsContext>(options =>
                options.UseMySql(connectionString,
                    new MySqlServerVersion(new Version(8, 0, 21)),
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableStringComparisonTranslations();
                        mySqlOptions.EnableRetryOnFailure();
                    }
                ));
        }
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        services.AddScoped<IFuelTypeRepository, FuelTypeRepository>();
        services.AddScoped<ITransmissionTypeRepository, TransmissionTypeRepository>();
        services.AddScoped<IServiceBookingRepository, ServiceBookingRepository>();

        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
