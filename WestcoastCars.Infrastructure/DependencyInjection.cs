using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Clients;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Options;
using WestcoastCars.Infrastructure.Repositories;
using WestcoastCars.Infrastructure.BackgroundJobs;
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

        services.AddHostedService<OutboxProcessor>();

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
