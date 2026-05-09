using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.FuelTypes.Commands.Delete;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.FuelTypes.Commands;

public class DeleteFuelTypeCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFuelTypeRepository> _fuelTypeRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly DeleteFuelTypeCommandHandler _handler;

    public DeleteFuelTypeCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fuelTypeRepositoryMock = new Mock<IFuelTypeRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _unitOfWorkMock.Setup(u => u.FuelTypeRepository).Returns(_fuelTypeRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _handler = new DeleteFuelTypeCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFuelType_WhenNoVehiclesAssigned()
    {
        var fuelTypeId = 1;
        var fuelType = new FuelType { Id = fuelTypeId, Name = "Petrol" };

        _fuelTypeRepositoryMock.Setup(r => r.GetByIdAsync(fuelTypeId)).ReturnsAsync(fuelType);
        _vehicleRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Vehicle, bool>>>())).ReturnsAsync((Vehicle?)null);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        await _handler.Handle(new DeleteFuelTypeCommand { Id = fuelTypeId }, CancellationToken.None);

        _fuelTypeRepositoryMock.Verify(r => r.Remove(fuelType), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenVehiclesAssigned()
    {
        var fuelTypeId = 1;
        var fuelType = new FuelType { Id = fuelTypeId, Name = "Petrol" };
        var vehicle = new Vehicle
        {
            RegistrationNumber = "ABC123", Model = "XC60", ModelYear = 2022,
            ImageUrl = "img.png", Description = "test",
            Manufacturer = new Manufacturer { Name = "Volvo" }, FuelType = fuelType,
            TransmissionType = new TransmissionType { Name = "Auto" }
        };

        _fuelTypeRepositoryMock.Setup(r => r.GetByIdAsync(fuelTypeId)).ReturnsAsync(fuelType);
        _vehicleRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Vehicle, bool>>>())).ReturnsAsync(vehicle);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new DeleteFuelTypeCommand { Id = fuelTypeId }, CancellationToken.None));

        _fuelTypeRepositoryMock.Verify(r => r.Remove(It.IsAny<FuelType>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenFuelTypeDoesNotExist()
    {
        _fuelTypeRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FuelType?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new DeleteFuelTypeCommand { Id = 99 }, CancellationToken.None));
    }
}
