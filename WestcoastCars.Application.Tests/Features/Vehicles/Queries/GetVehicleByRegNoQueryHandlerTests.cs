using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Vehicles.Queries.GetByRegNo;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Queries;

public class GetVehicleByRegNoQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly GetVehicleByRegNoQueryHandler _handler;

    public GetVehicleByRegNoQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new GetVehicleByRegNoQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenVehicleFound()
    {
        var vehicle = new Vehicle
        {
            RegistrationNumber = "ABC123",
            Model = "XC60",
            ModelYear = 2022,
            ImageUrl = "img.png",
            Description = "test",
            Manufacturer = new Manufacturer { Name = "Volvo" },
            FuelType = new FuelType { Name = "Petrol" },
            TransmissionType = new TransmissionType { Name = "Auto" }
        };

        _repositoryMock.Setup(r => r.FindByRegistrationNumberAsync("ABC123")).ReturnsAsync(vehicle);

        var result = await _handler.Handle(
            new GetVehicleByRegNoQuery { RegistrationNumber = "ABC123" }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("ABC123", result.RegistrationNumber);
        Assert.Equal("XC60", result.Model);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVehicleNotFound()
    {
        _repositoryMock.Setup(r => r.FindByRegistrationNumberAsync("XYZ999")).ReturnsAsync((Vehicle?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(
                new GetVehicleByRegNoQuery { RegistrationNumber = "XYZ999" }, CancellationToken.None));
    }
}
