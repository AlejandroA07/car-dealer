using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Vehicles.Commands.MarkAsSold;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;
using MediatR;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class MarkAsSoldCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly MarkAsSoldCommandHandler _handler;

    public MarkAsSoldCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _handler = new MarkAsSoldCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldMarkAsSold_WhenVehicleExists()
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
            TransmissionType = new TransmissionType { Name = "Trans" },
            IsSold = false
        };

        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync(vehicle);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new MarkAsSoldCommand { Id = vehicleId }, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        Assert.True(vehicle.IsSold);
        _vehicleRepositoryMock.Verify(r => r.Update(vehicle), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVehicleDoesNotExist()
    {
        // Arrange
        var vehicleId = 99;
        _vehicleRepositoryMock.Setup(r => r.GetByIdAsync(vehicleId)).ReturnsAsync((Vehicle?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new MarkAsSoldCommand { Id = vehicleId }, CancellationToken.None));
    }
}
