
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
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");

        if (connectionString is not null && password is not null)
        {
            connectionString = connectionString.Replace("${MYSQL_PASSWORD}", password);
        }

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
        services.AddScoped<IServiceBookingRepository, ServiceBookingRepository>();

        return services;
    }
}
