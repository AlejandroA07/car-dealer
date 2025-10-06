
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Repositories;
using WestcoastCars.Application.Exceptions;

namespace westcoast_cars.api.tests;

public class VehiclesControllerTests
{
    private readonly DbContextOptions<WestcoastCarsContext> _options;
    private readonly WestcoastCarsContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly VehiclesController _controller;

    public VehiclesControllerTests()
    {
        _options = new DbContextOptionsBuilder<WestcoastCarsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new WestcoastCarsContext(_options);
        _unitOfWork = new UnitOfWork(_context);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()).Value).Returns("http://test.com");

        var mockLogger = new Mock<ILogger<VehiclesController>>();

        _controller = new VehiclesController(_unitOfWork, mockConfiguration.Object, mockLogger.Object);

        // Seed the database
        var manufacturer = new Manufacturer { Name = "Volvo" };
        var fuelType = new FuelType { Name = "Petrol" };
        var transmissionType = new TransmissionType { Name = "Automatic" };

        _context.Manufacturers.Add(manufacturer);
        _context.FuelTypes.Add(fuelType);
        _context.TransmissionTypes.Add(transmissionType);

        _context.Vehicles.Add(new Vehicle
        {
            RegistrationNumber = "TEST123",
            Model = "V60",
            ModelYear = "2020",
            Mileage = 1000,
            ImageUrl = "/images/no-car.png",
            Description = "A test car",
            Manufacturer = manufacturer,
            FuelType = fuelType,
            TransmissionType = transmissionType
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task ListAll_ShouldReturnOkAndListOfVehicles()
    {
        // Arrange

        // Act
        var result = await _controller.ListAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var vehicles = Assert.IsAssignableFrom<IEnumerable<VehicleSummaryDto>>(okResult.Value);
        Assert.Single(vehicles);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkAndVehicle_WhenVehicleExists()
    {
        // Arrange
        var existingVehicleId = 1;

        // Act
        var result = await _controller.GetById(existingVehicleId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var vehicle = Assert.IsType<VehicleDetailsDto>(okResult.Value);
        Assert.Equal(existingVehicleId, vehicle.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenVehicleDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(nonExistentId));
    }

    [Fact]
    public async Task Add_ShouldCreateVehicleAndReturnCreatedAtAction()
    {
        // Arrange
        var newVehicleDto = new VehiclePostDto
        {
            RegistrationNumber = "NEWCAR1",
            Model = "Model S",
            ModelYear = "2023",
            Mileage = 500,
            Value = 90000,
            Description = "A new electric car",
            ImageUrl = "/images/no-car.png",
            ManufacturerId = 1,
            FuelTypeId = 1,
            TransmissionTypeId = 1
        };

        // Act
        var result = await _controller.Add(newVehicleDto);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        var vehicleCount = await _context.Vehicles.CountAsync();
        Assert.Equal(2, vehicleCount);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldModifyVehicle_WhenDataIsValid()
    {
        // Arrange
        var existingVehicleId = 1;
        var updatedDescription = "This description has been updated.";
        var vehicleUpdateDto = new VehicleUpdateDto
        {
            Id = existingVehicleId,
            Description = updatedDescription,
            Model = "V60",
            ModelYear = "2020",
            ManufacturerId = 1,
            FuelTypeId = 1,
            TransmissionTypeId = 1
        };

        // Act
        var result = await _controller.UpdateVehicle(existingVehicleId, vehicleUpdateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var updatedVehicle = await _context.Vehicles.FindAsync(existingVehicleId);
        Assert.Equal(updatedDescription, updatedVehicle.Description);
    }

    [Fact]
    public async Task Delete_ShouldRemoveVehicle_WhenIdExists()
    {
        // Arrange
        var existingVehicleId = 1;

        // Act
        var result = await _controller.Delete(existingVehicleId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var vehicleCount = await _context.Vehicles.CountAsync();
        Assert.Equal(0, vehicleCount);
    }
}
