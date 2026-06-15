using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.Vehicles.Queries.Search;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Queries;

public class SearchVehiclesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly SearchVehiclesQueryHandler _handler;

    public SearchVehiclesQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new SearchVehiclesQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WithMappedItems()
    {
        var criteria = new VehicleSearchDto();
        var pagedVehicles = new PagedResult<Vehicle>
        {
            Items = [new Vehicle
            {
                RegistrationNumber = "ABC123", Model = "XC40", ModelYear = 2023, ImageUrl = "img.png",
                Description = "test", Manufacturer = new Manufacturer { Name = "Volvo" },
                FuelType = new FuelType { Name = "Petrol" }, TransmissionType = new TransmissionType { Name = "Manual" }
            }],
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        var mappedDtos = new List<VehicleSummaryDto> { new() };

        _repositoryMock.Setup(r => r.SearchAsync(criteria)).ReturnsAsync(pagedVehicles);
        _mapperMock.Setup(m => m.Map<List<VehicleSummaryDto>>(pagedVehicles.Items)).Returns(mappedDtos);

        var result = await _handler.Handle(new SearchVehiclesQuery(criteria), CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Same(mappedDtos, result.Items);
    }
}
