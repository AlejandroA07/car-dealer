using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Vehicles.Commands.Delete;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;
using MediatR;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class DeleteVehicleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly DeleteVehicleCommandHandler _handler;

    public DeleteVehicleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _handler = new DeleteVehicleCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteVehicle_WhenVehicleExists()
    {
        // Arrange
        var vehicleId = 1;
        var vehicle = new Vehicle 
        { 
            Id = vehicleId,
            RegistrationNumber = "TEST123",
            Model = "Test",
            ModelYear = "2020",
            ImageUrl = "test.png",
            Description = "Test",
            Manufacturer = new Manufacturer { Name = "Make" },
            FuelType = new FuelType { Name = "Fuel" },
            TransmissionType = new TransmissionType { Name = "Trans" }
        };

        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(vehicle);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteVehicleCommand { Id = vehicleId }, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _vehicleRepositoryMock.Verify(r => r.Remove(vehicle), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVehicleDoesNotExist()
    {
        // Arrange
        var vehicleId = 99;
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync((Vehicle?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new DeleteVehicleCommand { Id = vehicleId }, CancellationToken.None));
    }
}
