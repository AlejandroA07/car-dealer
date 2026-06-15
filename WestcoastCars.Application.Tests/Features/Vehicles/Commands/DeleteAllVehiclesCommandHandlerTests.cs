using System.Collections.ObjectModel;
using Moq;
using WestcoastCars.Application.Features.Vehicles.Commands.DeleteAll;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class DeleteAllVehiclesCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly DeleteAllVehiclesCommandHandler _handler;

    public DeleteAllVehiclesCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new DeleteAllVehiclesCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteAllVehicles_WhenVehiclesExist()
    {
        var vehicles = BuildVehicles(3);

        _repositoryMock.Setup(r => r.GetAllForDeleteAsync()).ReturnsAsync(vehicles);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteAllVehiclesCommand(), CancellationToken.None);

        Assert.Equal(3, result.TotalDeleted);
        _repositoryMock.Verify(r => r.RemoveRange(vehicles), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteOrSave_WhenNoVehiclesExist()
    {
        _repositoryMock.Setup(r => r.GetAllForDeleteAsync()).ReturnsAsync(new List<Vehicle>().AsReadOnly());

        var result = await _handler.Handle(new DeleteAllVehiclesCommand(), CancellationToken.None);

        Assert.Equal(0, result.TotalDeleted);
        _repositoryMock.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<Vehicle>>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    private static ReadOnlyCollection<Vehicle> BuildVehicles(int count) =>
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
