using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class CreateVehicleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IManufacturerRepository> _manufacturerRepositoryMock;
    private readonly Mock<IFuelTypeRepository> _fuelTypeRepositoryMock;
    private readonly Mock<ITransmissionTypeRepository> _transmissionTypeRepositoryMock;
    private readonly CreateVehicleCommandHandler _handler;

    public CreateVehicleCommandHandlerTests()
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

        _handler = new CreateVehicleCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnVehicleId_WhenCreationIsSuccessful()
    {
        // Arrange
        var command = new CreateVehicleCommand
        {
            RegistrationNumber = "NEW123",
            ManufacturerId = 1,
            FuelTypeId = 1,
            TransmissionTypeId = 1,
            Model = "V60",
            ModelYear = "2024",
            Value = 450000,
            Description = "Test description",
            ImageUrl = "test.png"
        };

        _vehicleRepositoryMock.Setup(r => r.FindByRegistrationNumberAsync(command.RegistrationNumber))
            .ReturnsAsync((Vehicle?)null);
        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(command.ManufacturerId))
            .ReturnsAsync(new Manufacturer { Id = 1, Name = "Volvo" });
        _fuelTypeRepositoryMock.Setup(r => r.GetByIdAsync(command.FuelTypeId))
            .ReturnsAsync(new FuelType { Id = 1, Name = "Diesel" });
        _transmissionTypeRepositoryMock.Setup(r => r.GetByIdAsync(command.TransmissionTypeId))
            .ReturnsAsync(new TransmissionType { Id = 1, Name = "Automatic" });
        
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result >= 0);
        _vehicleRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Vehicle>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenVehicleAlreadyExists()
    {
        // Arrange
        var command = new CreateVehicleCommand { RegistrationNumber = "EXISTING" };
        var existingVehicle = new Vehicle 
        { 
            RegistrationNumber = "EXISTING",
            Model = "Test",
            ModelYear = "2020",
            ImageUrl = "test.png",
            Description = "Test",
            Manufacturer = new Manufacturer { Name = "Test" },
            FuelType = new FuelType { Name = "Test" },
            TransmissionType = new TransmissionType { Name = "Test" }
        };

        _vehicleRepositoryMock.Setup(r => r.FindByRegistrationNumberAsync(command.RegistrationNumber))
            .ReturnsAsync(existingVehicle);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenManufacturerDoesNotExist()
    {
        // Arrange
        var command = new CreateVehicleCommand { RegistrationNumber = "NEW123", ManufacturerId = 99 };
        _vehicleRepositoryMock.Setup(r => r.FindByRegistrationNumberAsync(command.RegistrationNumber))
            .ReturnsAsync((Vehicle?)null);
        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(command.ManufacturerId))
            .ReturnsAsync((Manufacturer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Manufacturer", exception.Message);
    }
}
