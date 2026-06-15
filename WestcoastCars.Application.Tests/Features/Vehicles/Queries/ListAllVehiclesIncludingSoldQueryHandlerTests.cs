using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.Vehicles.Queries.ListAll;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Queries;

public class ListAllVehiclesIncludingSoldQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ListAllVehiclesIncludingSoldQueryHandler _handler;

    public ListAllVehiclesIncludingSoldQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new ListAllVehiclesIncludingSoldQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WithMappedItems()
    {
        var pagedVehicles = new PagedResult<Vehicle>
        {
            Items = [new Vehicle
            {
                RegistrationNumber = "XYZ999", Model = "V90", ModelYear = 2021, ImageUrl = "img.png",
                Description = "test", Manufacturer = new Manufacturer { Name = "Volvo" },
                FuelType = new FuelType { Name = "Diesel" }, TransmissionType = new TransmissionType { Name = "Auto" }
            }],
            TotalCount = 1,
            Page = 2,
            PageSize = 10
        };
        var mappedDtos = new List<VehicleSummaryDto> { new() };

        _repositoryMock
            .Setup(r => r.GetAllPagedAsync(It.IsAny<PagedQueryDto>()))
            .ReturnsAsync(pagedVehicles);
        _mapperMock
            .Setup(m => m.Map<List<VehicleSummaryDto>>(pagedVehicles.Items))
            .Returns(mappedDtos);

        var result = await _handler.Handle(
            new ListAllVehiclesIncludingSoldQuery { Page = 2, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Same(mappedDtos, result.Items);
    }
}
