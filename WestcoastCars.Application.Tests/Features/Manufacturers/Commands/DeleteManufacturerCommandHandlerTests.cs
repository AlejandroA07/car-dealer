using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Manufacturers.Commands.Delete;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Commands;

public class DeleteManufacturerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IManufacturerRepository> _manufacturerRepositoryMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly DeleteManufacturerCommandHandler _handler;

    public DeleteManufacturerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _manufacturerRepositoryMock = new Mock<IManufacturerRepository>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_manufacturerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _handler = new DeleteManufacturerCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteManufacturer_WhenNoVehiclesAssigned()
    {
        var manufacturerId = 1;
        var manufacturer = new Manufacturer { Id = manufacturerId, Name = "Volvo" };

        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync(manufacturer);
        _vehicleRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Vehicle, bool>>>())).ReturnsAsync((Vehicle?)null);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        await _handler.Handle(new DeleteManufacturerCommand { Id = manufacturerId }, CancellationToken.None);

        _manufacturerRepositoryMock.Verify(r => r.Remove(manufacturer), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenVehiclesAssigned()
    {
        var manufacturerId = 1;
        var manufacturer = new Manufacturer { Id = manufacturerId, Name = "Volvo" };
        var vehicle = new Vehicle
        {
            RegistrationNumber = "ABC123",
            Model = "XC60",
            ModelYear = 2022,
            ImageUrl = "img.png",
            Description = "test",
            Manufacturer = manufacturer,
            FuelType = new FuelType { Name = "Petrol" },
            TransmissionType = new TransmissionType { Name = "Auto" }
        };

        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync(manufacturer);
        _vehicleRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Vehicle, bool>>>())).ReturnsAsync(vehicle);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(new DeleteManufacturerCommand { Id = manufacturerId }, CancellationToken.None));

        _manufacturerRepositoryMock.Verify(r => r.Remove(It.IsAny<Manufacturer>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenSaveAffectsNoRows()
    {
        var manufacturerId = 1;
        var manufacturer = new Manufacturer { Id = manufacturerId, Name = "Volvo" };

        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync(manufacturer);
        _vehicleRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Vehicle, bool>>>())).ReturnsAsync((Vehicle?)null);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        await Assert.ThrowsAsync<PersistenceException>(() => _handler.Handle(new DeleteManufacturerCommand { Id = manufacturerId }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenManufacturerDoesNotExist()
    {
        var manufacturerId = 99;
        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync((Manufacturer?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new DeleteManufacturerCommand { Id = manufacturerId }, CancellationToken.None));
    }
}
