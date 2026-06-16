using Microsoft.Extensions.Logging;
using Moq;
using WestcoastCars.Application.Features.Vehicles.Queries.PreviewBlocketVehicles;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Queries;

public class PreviewBlocketVehiclesQueryHandlerTests
{
    private readonly Mock<IBlocketApiClient> _apiClientMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _vehicleRepoMock = new();
    private readonly Mock<ILogger<PreviewBlocketVehiclesQueryHandler>> _loggerMock = new();
    private readonly PreviewBlocketVehiclesQueryHandler _handler;

    public PreviewBlocketVehiclesQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_vehicleRepoMock.Object);
        _vehicleRepoMock.Setup(r => r.GetBlocketVehicleIndexAsync())
            .ReturnsAsync(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
        _handler = new PreviewBlocketVehiclesQueryHandler(
            _apiClientMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenSearchReturnsNoDocs()
    {
        _apiClientMock
            .Setup(c => c.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse());

        var result = await _handler.Handle(new PreviewBlocketVehiclesQuery { Limit = 5 }, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldFilterOutAlreadyImportedVehicles()
    {
        var importedVehicle = new Vehicle
        {
            ExternalListingId = "EXT-EXISTING",
            Model = "XC60",
            ModelYear = 2022,
            ImageUrl = "img.png",
            Description = "x",
            Manufacturer = new Manufacturer { Name = "Volvo" },
            FuelType = new FuelType { Name = "Petrol" },
            TransmissionType = new TransmissionType { Name = "Auto" }
        };
        _vehicleRepoMock.Setup(r => r.GetBlocketVehicleIndexAsync())
            .ReturnsAsync(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["EXT-EXISTING"] = 1 });

        _apiClientMock
            .SetupSequence(c => c.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse
            {
                Docs =
                [
                    new BlocketCarSearchItem { Id = "EXT-EXISTING", Year = 2022 },
                    new BlocketCarSearchItem { Id = "EXT-NEW", Year = 2023 }
                ]
            })
            .ReturnsAsync(new BlocketCarSearchResponse());

        var result = await _handler.Handle(new PreviewBlocketVehiclesQuery { Limit = 5 }, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("EXT-NEW", result[0].ExternalListingId);
    }

    [Fact]
    public async Task Handle_ShouldSkipItemsWithInvalidModelYear()
    {
        _apiClientMock
            .SetupSequence(c => c.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse
            {
                Docs =
                [
                    new BlocketCarSearchItem { Id = "A", Year = 1899 },
                    new BlocketCarSearchItem { Id = "B", Year = null },
                    new BlocketCarSearchItem { Id = "C", Year = 2022 }
                ]
            })
            .ReturnsAsync(new BlocketCarSearchResponse());

        var result = await _handler.Handle(new PreviewBlocketVehiclesQuery { Limit = 5 }, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("C", result[0].ExternalListingId);
    }

    [Fact]
    public async Task Handle_ShouldClampLimitTo50()
    {
        var docs = Enumerable.Range(1, 60)
            .Select(i => new BlocketCarSearchItem { Id = i.ToString(), Year = 2022 })
            .ToList();

        _apiClientMock
            .Setup(c => c.SearchCarsAsync(It.IsAny<BlocketCarSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlocketCarSearchResponse { Docs = docs });

        var result = await _handler.Handle(new PreviewBlocketVehiclesQuery { Limit = 999 }, CancellationToken.None);

        Assert.Equal(50, result.Count);
    }
}
