using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Vehicles.Commands.Update;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;
using MediatR;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class UpdateVehicleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IManufacturerRepository> _manufacturerRepositoryMock;
    private readonly Mock<IFuelTypeRepository> _fuelTypeRepositoryMock;
    private readonly Mock<ITransmissionTypeRepository> _transmissionTypeRepositoryMock;
    private readonly UpdateVehicleCommandHandler _handler;

    public UpdateVehicleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _manufacturerRepositoryMock = new Mock<IManufacturerRepository>();
        _fuelTypeRepositoryMock = new Mock<IFuelTypeRepository>();
        _transmissionTypeRepositoryMock = new Mock<ITransmissionTypeRepository>();

        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_manufacturerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.FuelTypeRepository).Returns(_fuelTypeRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_transmissionTypeRepositoryMock.Object);

        _handler = new UpdateVehicleCommandHandler(_unitOfWorkMock.Object);
    }

    private Vehicle CreateTestVehicle(int id, string regNo)
    {
        return new Vehicle
        {
            Id = id,
            RegistrationNumber = regNo,
            Model = "Test Model",
            ModelYear = "2020",
            ImageUrl = "test.png",
            Description = "Test Description",
            Manufacturer = new Manufacturer { Id = 1, Name = "Test Make" },
            FuelType = new FuelType { Id = 1, Name = "Test Fuel" },
            TransmissionType = new TransmissionType { Id = 1, Name = "Test Trans" }
        };
    }

    [Fact]
    public async Task Handle_ShouldUpdateVehicle_WhenUpdateIsSuccessful()
    {
        // Arrange
        var vehicleId = 1;
        var command = new UpdateVehicleCommand
        {
            Id = vehicleId,
            RegistrationNumber = "UPDATED",
            ManufacturerId = 1,
            FuelTypeId = 1,
            TransmissionTypeId = 1,
            Model = "New Model",
            ModelYear = "2024",
            Value = 500000,
            Description = "New Description"
        };

        var existingVehicle = CreateTestVehicle(vehicleId, "OLD123");

        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(existingVehicle);
        _vehicleRepositoryMock.Setup(r => r.FindByRegistrationNumberAsync(command.RegistrationNumber)).ReturnsAsync((Vehicle?)null);
        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(command.ManufacturerId)).ReturnsAsync(new Manufacturer { Id = 1, Name = "Make" });
        _fuelTypeRepositoryMock.Setup(r => r.GetByIdAsync(command.FuelTypeId)).ReturnsAsync(new FuelType { Id = 1, Name = "Fuel" });
        _transmissionTypeRepositoryMock.Setup(r => r.GetByIdAsync(command.TransmissionTypeId)).ReturnsAsync(new TransmissionType { Id = 1, Name = "Trans" });
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        Assert.Equal(command.RegistrationNumber, existingVehicle.RegistrationNumber);
        Assert.Equal(command.Model, existingVehicle.Model);
        _vehicleRepositoryMock.Verify(r => r.Update(existingVehicle), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVehicleDoesNotExist()
    {
        // Arrange
        var command = new UpdateVehicleCommand { Id = 99 };
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Vehicle?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenRegistrationNumberExistsForOtherVehicle()
    {
        // Arrange
        var vehicleId = 1;
        var command = new UpdateVehicleCommand { Id = vehicleId, RegistrationNumber = "EXISTING" };
        var existingVehicle = CreateTestVehicle(vehicleId, "OLD");
        var otherVehicle = CreateTestVehicle(2, "EXISTING");

        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(existingVehicle);
        _vehicleRepositoryMock.Setup(r => r.FindByRegistrationNumberAsync(command.RegistrationNumber)).ReturnsAsync(otherVehicle);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
