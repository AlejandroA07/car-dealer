using Moq;
using WestcoastCars.Application.Features.Vehicles.Commands.BulkDelete;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class BulkDeleteVehiclesCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly BulkDeleteVehiclesCommandHandler _handler;

    public BulkDeleteVehiclesCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new BulkDeleteVehiclesCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNoFiltersSpecified()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new BulkDeleteVehiclesCommand(), CancellationToken.None));

        _repositoryMock.Verify(r => r.GetForBulkDeleteAsync(
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<int?>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteVehicles_WhenMatchingVehiclesFound()
    {
        var vehicles = BuildVehicles(2);

        _repositoryMock
            .Setup(r => r.GetForBulkDeleteAsync("Volvo", null, null, null, null))
            .ReturnsAsync(vehicles);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(
            new BulkDeleteVehiclesCommand { Make = "Volvo" }, CancellationToken.None);

        Assert.Equal(2, result.TotalDeleted);
        _repositoryMock.Verify(r => r.RemoveRange(vehicles), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteOrSave_WhenNoMatchingVehiclesFound()
    {
        _repositoryMock
            .Setup(r => r.GetForBulkDeleteAsync(null, "XC60", null, null, null))
            .ReturnsAsync(new List<Vehicle>().AsReadOnly());

        var result = await _handler.Handle(
            new BulkDeleteVehiclesCommand { Model = "XC60" }, CancellationToken.None);

        Assert.Equal(0, result.TotalDeleted);
        _repositoryMock.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<Vehicle>>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    private static IReadOnlyList<Vehicle> BuildVehicles(int count) =>
        Enumerable.Range(1, count).Select(i => new Vehicle
        {
            RegistrationNumber = $"REG{i}",
            Model = "XC60",
            ModelYear = 2022,
            ImageUrl = "img.png",
            Description = "test",
            Manufacturer = new Manufacturer { Name = "Volvo" },
            FuelType = new FuelType { Name = "Petrol" },
            TransmissionType = new TransmissionType { Name = "Auto" }
        }).ToList().AsReadOnly();
}
