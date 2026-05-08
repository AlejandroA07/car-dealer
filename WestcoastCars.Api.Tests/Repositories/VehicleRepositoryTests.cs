using Microsoft.EntityFrameworkCore;
using WestcoastCars.Contracts.DTOs;
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

    [Fact]
    public async Task GetAllForReplacementAsync_ShouldReturnOnlyBlocketVehicles()
    {
        var databaseName = Guid.NewGuid().ToString();
        await SeedVehicleAsync(databaseName, "BLK123", "Blocket");
        await SeedVehicleAsync(databaseName, "MAN123", null);

        await using var queryContext = CreateContext(databaseName);
        var repository = new VehicleRepository(queryContext);

        var vehicles = (await repository.GetAllForReplacementAsync()).ToList();

        Assert.Single(vehicles);
        Assert.Equal("BLK123", vehicles[0].RegistrationNumber);
        Assert.Equal("Blocket", vehicles[0].Source);
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByModelYearRange()
    {
        var databaseName = Guid.NewGuid().ToString();
        await SeedVehicleAsync(databaseName, "OLD123", modelYear: 2019);
        await SeedVehicleAsync(databaseName, "MID123", modelYear: 2022);
        await SeedVehicleAsync(databaseName, "NEW123", modelYear: 2025);

        await using var queryContext = CreateContext(databaseName);
        var repository = new VehicleRepository(queryContext);

        var result = await repository.SearchAsync(new VehicleSearchDto
        {
            MinYear = 2020,
            MaxYear = 2024
        });

        Assert.Single(result.Items);
        Assert.Equal("MID123", result.Items[0].RegistrationNumber);
    }

    private static async Task<int> SeedVehicleAsync(
        string databaseName,
        string registrationNumber = "ABC123",
        string? source = null,
        int modelYear = 2024)
    {
        await using var seedContext = CreateContext(databaseName);

        var manufacturer = new Manufacturer { Name = "Volvo" };
        var fuelType = new FuelType { Name = "Diesel" };
        var transmissionType = new TransmissionType { Name = "Automatic" };
        var vehicle = new Vehicle
        {
            RegistrationNumber = registrationNumber,
            Model = "XC60",
            ModelYear = modelYear,
            Mileage = 1000,
            ImageUrl = "/images/xc60.png",
            Price = 500000,
            Description = "Repository test vehicle",
            Manufacturer = manufacturer,
            FuelType = fuelType,
            TransmissionType = transmissionType,
            Source = source
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
