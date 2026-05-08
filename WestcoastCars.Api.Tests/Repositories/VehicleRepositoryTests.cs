using Microsoft.EntityFrameworkCore;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Repositories;
using Xunit;

namespace WestcoastCars.Api.Tests.Repositories;

public class VehicleRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldEagerLoadNavigationProperties()
    {
        var databaseName = Guid.NewGuid().ToString();
        var vehicleId = await SeedVehicleAsync(databaseName);

        await using var queryContext = CreateContext(databaseName);
        var repository = new VehicleRepository(queryContext);

        var vehicle = await repository.GetByIdAsync(vehicleId);

        Assert.NotNull(vehicle);
        Assert.NotNull(vehicle!.Manufacturer);
        Assert.NotNull(vehicle.FuelType);
        Assert.NotNull(vehicle.TransmissionType);
        Assert.Equal("Volvo", vehicle.Manufacturer.Name);
        Assert.Equal("Diesel", vehicle.FuelType.Name);
        Assert.Equal("Automatic", vehicle.TransmissionType.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldEagerLoadNavigationProperties()
    {
        var databaseName = Guid.NewGuid().ToString();
        await SeedVehicleAsync(databaseName);

        await using var queryContext = CreateContext(databaseName);
        var repository = new VehicleRepository(queryContext);

        var vehicles = (await repository.GetAllAsync()).ToList();

        Assert.Single(vehicles);
        Assert.All(vehicles, vehicle =>
        {
            Assert.NotNull(vehicle.Manufacturer);
            Assert.NotNull(vehicle.FuelType);
            Assert.NotNull(vehicle.TransmissionType);
        });
        Assert.Equal("Volvo", vehicles[0].Manufacturer.Name);
        Assert.Equal("Diesel", vehicles[0].FuelType.Name);
        Assert.Equal("Automatic", vehicles[0].TransmissionType.Name);
    }

    private static async Task<int> SeedVehicleAsync(string databaseName)
    {
        await using var seedContext = CreateContext(databaseName);

        var manufacturer = new Manufacturer { Name = "Volvo" };
        var fuelType = new FuelType { Name = "Diesel" };
        var transmissionType = new TransmissionType { Name = "Automatic" };
        var vehicle = new Vehicle
        {
            RegistrationNumber = "ABC123",
            Model = "XC60",
            ModelYear = "2024",
            Mileage = 1000,
            ImageUrl = "/images/xc60.png",
            Value = 500000,
            Description = "Repository test vehicle",
            Manufacturer = manufacturer,
            FuelType = fuelType,
            TransmissionType = transmissionType
        };

        seedContext.Manufacturers.Add(manufacturer);
        seedContext.FuelTypes.Add(fuelType);
        seedContext.TransmissionTypes.Add(transmissionType);
        seedContext.Vehicles.Add(vehicle);
        await seedContext.SaveChangesAsync();

        return vehicle.Id;
    }

    private static WestcoastCarsContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<WestcoastCarsContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new WestcoastCarsContext(options);
    }
}
