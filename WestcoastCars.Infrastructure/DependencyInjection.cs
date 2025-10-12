
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Repositories;

namespace WestcoastCars.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<WestcoastCarsContext>>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");

        logger.LogInformation("Connection string from config: {connectionString}", connectionString);
        logger.LogInformation("Password from env: {password}", password);

        if (connectionString is not null && password is not null) 
        {
            connectionString = connectionString.Replace("${MYSQL_PASSWORD}", password);
        }

        logger.LogInformation("Final connection string: {connectionString}", connectionString);

        services.AddDbContext<WestcoastCarsContext>(options =>
            options.UseMySql(connectionString,
                new MySqlServerVersion(new Version(8, 0, 21)),
                mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()
            ));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
        services.AddScoped<IFuelTypeRepository, FuelTypeRepository>();
        services.AddScoped<ITransmissionTypeRepository, TransmissionTypeRepository>();

        return services;
    }
}
