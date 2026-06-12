using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Transmissions.Commands.Delete;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Transmissions.Commands;

public class DeleteTransmissionCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITransmissionTypeRepository> _transmissionRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly DeleteTransmissionCommandHandler _handler;

    public DeleteTransmissionCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transmissionRepositoryMock = new Mock<ITransmissionTypeRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_transmissionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _handler = new DeleteTransmissionCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteTransmissionType_WhenNoVehiclesAssigned()
    {
        var id = 1;
        var transmissionType = new TransmissionType { Id = id, Name = "Automatic" };

        _transmissionRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(transmissionType);
        _vehicleRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Vehicle, bool>>>())).ReturnsAsync((Vehicle?)null);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        await _handler.Handle(new DeleteTransmissionCommand { Id = id }, CancellationToken.None);

        _transmissionRepositoryMock.Verify(r => r.Remove(transmissionType), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenVehiclesAssigned()
    {
        var id = 1;
        var transmissionType = new TransmissionType { Id = id, Name = "Automatic" };
        var vehicle = new Vehicle
        {
            RegistrationNumber = "ABC123",
            Model = "XC60",
            ModelYear = 2022,
            ImageUrl = "img.png",
            Description = "test",
            Manufacturer = new Manufacturer { Name = "Volvo" },
            FuelType = new FuelType { Name = "Petrol" },
            TransmissionType = transmissionType
        };

        _transmissionRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(transmissionType);
        _vehicleRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Vehicle, bool>>>())).ReturnsAsync(vehicle);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new DeleteTransmissionCommand { Id = id }, CancellationToken.None));

        _transmissionRepositoryMock.Verify(r => r.Remove(It.IsAny<TransmissionType>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTransmissionTypeDoesNotExist()
    {
        _transmissionRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TransmissionType?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new DeleteTransmissionCommand { Id = 99 }, CancellationToken.None));
    }
}
