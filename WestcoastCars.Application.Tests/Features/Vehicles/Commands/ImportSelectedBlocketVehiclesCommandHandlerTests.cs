using Moq;
using WestcoastCars.Application.Features.Vehicles.Commands.ImportSelectedBlocketVehicles;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class ImportSelectedBlocketVehiclesCommandHandlerTests
{
    private readonly Mock<IBlocketApiClient> _apiClientMock = new();
    private readonly Mock<IBlocketVehicleImportMapper> _mapperMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _vehicleRepoMock = new();
    private readonly Mock<IManufacturerRepository> _manufacturerRepoMock = new();
    private readonly Mock<IFuelTypeRepository> _fuelTypeRepoMock = new();
    private readonly Mock<ITransmissionTypeRepository> _transmissionRepoMock = new();
    private readonly ImportSelectedBlocketVehiclesCommandHandler _handler;

    public ImportSelectedBlocketVehiclesCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_vehicleRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_manufacturerRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.FuelTypeRepository).Returns(_fuelTypeRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_transmissionRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        _vehicleRepoMock.Setup(r => r.GetByExternalIdsAsync(It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync([]);
        _manufacturerRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([new Manufacturer { Name = "Volvo" }]);
        _fuelTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([new FuelType { Name = "Petrol" }]);
        _transmissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([new TransmissionType { Name = "Automatic" }]);

        _handler = new ImportSelectedBlocketVehiclesCommandHandler(
            _apiClientMock.Object, _mapperMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoIdsProvided()
    {
        var result = await _handler.Handle(new ImportSelectedBlocketVehiclesCommand(), CancellationToken.None);

        Assert.Equal(0, result.TotalSelected);
        Assert.Equal(0, result.TotalAdded);
        Assert.Equal(0, result.TotalUpdated);
        _apiClientMock.Verify(c => c.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipBlankIds()
    {
        var result = await _handler.Handle(
            new ImportSelectedBlocketVehiclesCommand { ExternalListingIds = ["", "  ", null!] },
            CancellationToken.None);

        Assert.Equal(3, result.TotalSkipped);
        Assert.Equal(0, result.TotalAdded);
        _apiClientMock.Verify(c => c.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAddNewVehicle_WhenIdNotAlreadyImported()
    {
        _apiClientMock
            .Setup(c => c.GetCarAdAsync("EXT-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());

        _mapperMock
            .Setup(m => m.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns(new BlocketVehicleImportData
            {
                ExternalListingId = "EXT-1",
                RegistrationNumber = "REG1",
                Manufacturer = "Volvo",
                FuelType = "Petrol",
                TransmissionType = "Automatic",
                Model = "XC60",
                ModelYear = 2022,
                ImageUrl = "img.png",
                Description = "test"
            });

        var result = await _handler.Handle(
            new ImportSelectedBlocketVehiclesCommand { ExternalListingIds = ["EXT-1"] },
            CancellationToken.None);

        Assert.Equal(1, result.TotalSelected);
        Assert.Equal(1, result.TotalAdded);
        Assert.Equal(0, result.TotalUpdated);
        _vehicleRepoMock.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<Vehicle>>(v => v.Count() == 1)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingVehicle_WhenIdAlreadyImported()
    {
        var existing = new Vehicle
        {
            ExternalListingId = "EXT-2",
            RegistrationNumber = "REG2",
            Model = "XC60",
            ModelYear = 2022,
            ImageUrl = "img.png",
            Description = "old",
            Price = 100_000,
            Mileage = 50_000,
            Manufacturer = new Manufacturer { Name = "Volvo" },
            FuelType = new FuelType { Name = "Petrol" },
            TransmissionType = new TransmissionType { Name = "Automatic" }
        };

        _vehicleRepoMock.Setup(r => r.GetByExternalIdsAsync(It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync([existing]);
        _apiClientMock
            .Setup(c => c.GetCarAdAsync("EXT-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());
        _mapperMock
            .Setup(m => m.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns(new BlocketVehicleImportData
            {
                ExternalListingId = "EXT-2",
                RegistrationNumber = "REG2",
                Manufacturer = "Volvo",
                FuelType = "Petrol",
                TransmissionType = "Automatic",
                Model = "XC60",
                ModelYear = 2022,
                ImageUrl = "img.png",
                Description = "updated",
                Price = 95_000,
                Mileage = 55_000
            });

        var result = await _handler.Handle(
            new ImportSelectedBlocketVehiclesCommand { ExternalListingIds = ["EXT-2"] },
            CancellationToken.None);

        Assert.Equal(0, result.TotalAdded);
        Assert.Equal(1, result.TotalUpdated);
        Assert.Equal(95_000, existing.Price);
        Assert.Equal(55_000, existing.Mileage);
        _vehicleRepoMock.Verify(r => r.Update(existing), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipVehicle_WhenModelYearIsNull()
    {
        _apiClientMock
            .Setup(c => c.GetCarAdAsync("EXT-3", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());
        _mapperMock
            .Setup(m => m.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns(new BlocketVehicleImportData
            {
                ExternalListingId = "EXT-3",
                ModelYear = null,
                Manufacturer = "Volvo",
                FuelType = "Petrol",
                TransmissionType = "Automatic",
                Model = "XC60",
                ImageUrl = "img.png",
                Description = "no year"
            });

        var result = await _handler.Handle(
            new ImportSelectedBlocketVehiclesCommand { ExternalListingIds = ["EXT-3"] },
            CancellationToken.None);

        Assert.Equal(1, result.TotalSkipped);
        Assert.Equal(0, result.TotalAdded);
    }
}
