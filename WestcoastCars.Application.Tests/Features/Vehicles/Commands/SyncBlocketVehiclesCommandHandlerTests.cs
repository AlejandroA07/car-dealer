using Moq;
using WestcoastCars.Application.Features.Vehicles.Commands.SyncBlocket;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class SyncBlocketVehiclesCommandHandlerTests
{
    private readonly Mock<IBlocketApiClient> _blocketApiClientMock;
    private readonly Mock<IBlocketVehicleImportMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IManufacturerRepository> _manufacturerRepositoryMock;
    private readonly Mock<IFuelTypeRepository> _fuelTypeRepositoryMock;
    private readonly Mock<ITransmissionTypeRepository> _transmissionTypeRepositoryMock;
    private readonly SyncBlocketVehiclesCommandHandler _handler;

    public SyncBlocketVehiclesCommandHandlerTests()
    {
        _blocketApiClientMock = new Mock<IBlocketApiClient>();
        _mapperMock = new Mock<IBlocketVehicleImportMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _manufacturerRepositoryMock = new Mock<IManufacturerRepository>();
        _fuelTypeRepositoryMock = new Mock<IFuelTypeRepository>();
        _transmissionTypeRepositoryMock = new Mock<ITransmissionTypeRepository>();

        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.ManufacturerRepository).Returns(_manufacturerRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.FuelTypeRepository).Returns(_fuelTypeRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.TransmissionTypeRepository).Returns(_transmissionTypeRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.CompleteAsync()).ReturnsAsync(1);

        SetupLookupRepositories();

        _handler = new SyncBlocketVehiclesCommandHandler(_blocketApiClientMock.Object, _mapperMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldClampLimitTo50()
    {
        var docs = Enumerable.Range(1, 60)
            .Select(index => new BlocketCarSearchItem { Id = index.ToString() })
            .ToList();

        _blocketApiClientMock
            .Setup(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse { Docs = docs });

        _blocketApiClientMock
            .Setup(client => client.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());

        _mapperMock
            .Setup(mapper => mapper.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns<BlocketCarSearchItem, BlocketCarAdDetails, DateTime>((item, _, importedAt) => new BlocketVehicleImportData
            {
                ExternalListingId = item.Id,
                RegistrationNumber = $"REG{item.Id}",
                ImportedAt = importedAt,
                Manufacturer = "VOLVO",
                Model = $"Model {item.Id}",
                ModelYear = "2024"
            });

        var result = await _handler.Handle(new SyncBlocketVehiclesCommand { Limit = 999 }, CancellationToken.None);

        Assert.Equal(999, result.RequestedLimit);
        Assert.Equal(50, result.AppliedLimit);
        Assert.Equal(50, result.TotalPrepared);
        Assert.Equal(50, result.TotalImported);
        _blocketApiClientMock.Verify(client => client.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(50));
    }

    [Fact]
    public async Task Handle_ShouldFetchMultiplePagesUntilLimitIsReached()
    {
        _blocketApiClientMock
            .SetupSequence(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse
            {
                Docs =
                [
                    new BlocketCarSearchItem { Id = "1" },
                    new BlocketCarSearchItem { Id = "2" }
                ]
            })
            .ReturnsAsync(new BlocketCarSearchResponse
            {
                Docs =
                [
                    new BlocketCarSearchItem { Id = "3" },
                    new BlocketCarSearchItem { Id = "4" }
                ]
            });

        _blocketApiClientMock
            .Setup(client => client.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());

        _mapperMock
            .Setup(mapper => mapper.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns<BlocketCarSearchItem, BlocketCarAdDetails, DateTime>((item, _, importedAt) => new BlocketVehicleImportData
            {
                ExternalListingId = item.Id,
                RegistrationNumber = $"REG{item.Id}",
                ImportedAt = importedAt,
                Manufacturer = "AUDI",
                Model = $"Model {item.Id}",
                ModelYear = "2024"
            });

        var result = await _handler.Handle(new SyncBlocketVehiclesCommand { Limit = 3 }, CancellationToken.None);

        Assert.Equal(2, result.PagesFetched);
        Assert.Equal(4, result.TotalFetched);
        Assert.Equal(3, result.TotalPrepared);
        Assert.Collection(result.Vehicles,
            vehicle => Assert.Equal("1", vehicle.ExternalListingId),
            vehicle => Assert.Equal("2", vehicle.ExternalListingId),
            vehicle => Assert.Equal("3", vehicle.ExternalListingId));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyBatch_WhenSearchReturnsNoDocs()
    {
        _blocketApiClientMock
            .Setup(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse());

        var result = await _handler.Handle(new SyncBlocketVehiclesCommand { Limit = 10 }, CancellationToken.None);

        Assert.Equal(1, result.PagesFetched);
        Assert.Equal(0, result.TotalFetched);
        Assert.Equal(0, result.TotalPrepared);
        Assert.Empty(result.Vehicles);
        _blocketApiClientMock.Verify(client => client.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _manufacturerRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Never);
        _fuelTypeRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Never);
        _transmissionTypeRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReplaceExistingVehicles()
    {
        var existingVehicles = new List<Vehicle>
        {
            new() { Id = 1, Source = "Blocket", RegistrationNumber = "OLD001", Model = "Old", ModelYear = "2020", ImageUrl = "x", Description = "x", Manufacturer = new Manufacturer { Name = "VOLVO" }, FuelType = new FuelType { Name = "Petrol" }, TransmissionType = new TransmissionType { Name = "Automatic" } },
            new() { Id = 2, Source = null, RegistrationNumber = "MAN001", Model = "Manual", ModelYear = "2021", ImageUrl = "x", Description = "x", Manufacturer = new Manufacturer { Name = "VOLVO" }, FuelType = new FuelType { Name = "Petrol" }, TransmissionType = new TransmissionType { Name = "Automatic" } }
        };

        _vehicleRepositoryMock
            .Setup(repository => repository.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Vehicle, bool>>>()))
            .ReturnsAsync([]);

        _vehicleRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(existingVehicles);

        _blocketApiClientMock
            .Setup(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse
            {
                Docs = [new BlocketCarSearchItem { Id = "10" }]
            });

        _blocketApiClientMock
            .SetupSequence(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse { Docs = [new BlocketCarSearchItem { Id = "10" }] })
            .ReturnsAsync(new BlocketCarSearchResponse());

        _blocketApiClientMock
            .Setup(client => client.GetCarAdAsync("10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());

        _mapperMock
            .Setup(mapper => mapper.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns(new BlocketVehicleImportData
            {
                ExternalListingId = "10",
                RegistrationNumber = "REG10",
                Manufacturer = "VOLVO",
                FuelType = "Petrol",
                TransmissionType = "Automatic",
                Model = "XC60",
                ModelYear = "2024",
                ImageUrl = "/images/no-car.png",
                Description = "Imported"
            });

        var result = await _handler.Handle(new SyncBlocketVehiclesCommand { Limit = 1 }, CancellationToken.None);

        Assert.Equal(2, result.TotalReplaced);
        Assert.Equal(1, result.TotalImported);
        _vehicleRepositoryMock.Verify(repository => repository.RemoveRange(It.Is<IEnumerable<Vehicle>>(vehicles => vehicles.Count() == 2)), Times.Once);
        _vehicleRepositoryMock.Verify(repository => repository.AddRangeAsync(It.Is<IEnumerable<Vehicle>>(vehicles => vehicles.Count() == 1)), Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteExistingImportedVehicles_WhenExternalFetchFails()
    {
        _blocketApiClientMock
            .Setup(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Boom"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _handler.Handle(new SyncBlocketVehiclesCommand(), CancellationToken.None));

        _vehicleRepositoryMock.Verify(repository => repository.RemoveRange(It.IsAny<IEnumerable<Vehicle>>()), Times.Never);
        _vehicleRepositoryMock.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Vehicle>>()), Times.Never);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipDuplicateExternalListingIds()
    {
        _blocketApiClientMock
            .SetupSequence(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse
            {
                Docs =
                [
                    new BlocketCarSearchItem { Id = "1" },
                    new BlocketCarSearchItem { Id = "1" },
                    new BlocketCarSearchItem { Id = "2" }
                ]
            })
            .ReturnsAsync(new BlocketCarSearchResponse());

        _blocketApiClientMock
            .Setup(client => client.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());

        _mapperMock
            .SetupSequence(mapper => mapper.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns(new BlocketVehicleImportData { ExternalListingId = "1", RegistrationNumber = "REG1", Manufacturer = "VOLVO", FuelType = "Petrol", TransmissionType = "Automatic", Model = "A", ModelYear = "2024", ImageUrl = "x", Description = "x" })
            .Returns(new BlocketVehicleImportData { ExternalListingId = "1", RegistrationNumber = "REG1", Manufacturer = "VOLVO", FuelType = "Petrol", TransmissionType = "Automatic", Model = "B", ModelYear = "2024", ImageUrl = "x", Description = "x" })
            .Returns(new BlocketVehicleImportData { ExternalListingId = "2", RegistrationNumber = "REG2", Manufacturer = "VOLVO", FuelType = "Petrol", TransmissionType = "Automatic", Model = "C", ModelYear = "2024", ImageUrl = "x", Description = "x" });

        var result = await _handler.Handle(new SyncBlocketVehiclesCommand { Limit = 3 }, CancellationToken.None);

        Assert.Equal(2, result.TotalImported);
        Assert.Equal(1, result.TotalSkipped);
    }

    [Fact]
    public async Task Handle_ShouldLoadLookupTablesOnceAndResolveExistingLookupsInMemory()
    {
        _blocketApiClientMock
            .SetupSequence(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse
            {
                Docs =
                [
                    new BlocketCarSearchItem { Id = "1" },
                    new BlocketCarSearchItem { Id = "2" }
                ]
            })
            .ReturnsAsync(new BlocketCarSearchResponse());

        _blocketApiClientMock
            .Setup(client => client.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());

        _mapperMock
            .Setup(mapper => mapper.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns<BlocketCarSearchItem, BlocketCarAdDetails, DateTime>((item, _, importedAt) => new BlocketVehicleImportData
            {
                ExternalListingId = item.Id,
                RegistrationNumber = $"REG{item.Id}",
                Manufacturer = "VOLVO",
                FuelType = "Petrol",
                TransmissionType = "Automatic",
                Model = $"Model {item.Id}",
                ModelYear = "2024",
                ImageUrl = "x",
                Description = "x",
                ImportedAt = importedAt
            });

        await _handler.Handle(new SyncBlocketVehiclesCommand { Limit = 2 }, CancellationToken.None);

        _manufacturerRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
        _fuelTypeRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
        _transmissionTypeRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
        _manufacturerRepositoryMock.Verify(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Manufacturer, bool>>>()), Times.Never);
        _fuelTypeRepositoryMock.Verify(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<FuelType, bool>>>()), Times.Never);
        _transmissionTypeRepositoryMock.Verify(repository => repository.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TransmissionType, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateEachMissingLookupOnlyOncePerBatch()
    {
        _manufacturerRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync([]);
        _fuelTypeRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync([]);
        _transmissionTypeRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync([]);

        _blocketApiClientMock
            .SetupSequence(client => client.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse
            {
                Docs =
                [
                    new BlocketCarSearchItem { Id = "1" },
                    new BlocketCarSearchItem { Id = "2" }
                ]
            })
            .ReturnsAsync(new BlocketCarSearchResponse());

        _blocketApiClientMock
            .Setup(client => client.GetCarAdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarAdDetails());

        _mapperMock
            .SetupSequence(mapper => mapper.Map(It.IsAny<BlocketCarSearchItem>(), It.IsAny<BlocketCarAdDetails>(), It.IsAny<DateTime>()))
            .Returns(new BlocketVehicleImportData
            {
                ExternalListingId = "1",
                RegistrationNumber = "REG1",
                Manufacturer = " Saab ",
                FuelType = "Diesel",
                TransmissionType = "Manual",
                Model = "A",
                ModelYear = "2024",
                ImageUrl = "x",
                Description = "x"
            })
            .Returns(new BlocketVehicleImportData
            {
                ExternalListingId = "2",
                RegistrationNumber = "REG2",
                Manufacturer = "SAAB",
                FuelType = " diesel ",
                TransmissionType = "manual",
                Model = "B",
                ModelYear = "2024",
                ImageUrl = "x",
                Description = "x"
            });

        await _handler.Handle(new SyncBlocketVehiclesCommand { Limit = 2 }, CancellationToken.None);

        _manufacturerRepositoryMock.Verify(repository => repository.AddAsync(It.Is<Manufacturer>(manufacturer => manufacturer.Name == "Saab")), Times.Once);
        _fuelTypeRepositoryMock.Verify(repository => repository.AddAsync(It.Is<FuelType>(fuelType => fuelType.Name == "Diesel")), Times.Once);
        _transmissionTypeRepositoryMock.Verify(repository => repository.AddAsync(It.Is<TransmissionType>(transmissionType => transmissionType.Name == "Manual")), Times.Once);
    }

    private void SetupLookupRepositories()
    {
        _vehicleRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync([]);

        _manufacturerRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync([new Manufacturer { Id = 2, Name = "VOLVO" }]);

        _fuelTypeRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync([new FuelType { Id = 2, Name = "Petrol" }]);

        _transmissionTypeRepositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync([new TransmissionType { Id = 2, Name = "Automatic" }]);
    }
}
