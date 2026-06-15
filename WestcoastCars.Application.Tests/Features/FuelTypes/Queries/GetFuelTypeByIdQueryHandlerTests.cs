using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.FuelTypes.Queries.GetById;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.FuelTypes.Queries;

public class GetFuelTypeByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFuelTypeRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GetFuelTypeByIdQueryHandler _handler;

    public GetFuelTypeByIdQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.FuelTypeRepository).Returns(_repositoryMock.Object);
        _handler = new GetFuelTypeByIdQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenFuelTypeFound()
    {
        var fuelType = new FuelType { Id = 1, Name = "Petrol" };
        var dto = new NamedObjectDto { Id = 1, Name = "Petrol" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fuelType);
        _mapperMock.Setup(m => m.Map<NamedObjectDto>(fuelType)).Returns(dto);

        var result = await _handler.Handle(new GetFuelTypeByIdQuery { Id = 1 }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(dto, result);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenFuelTypeNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FuelType?)null);

        var result = await _handler.Handle(new GetFuelTypeByIdQuery { Id = 99 }, CancellationToken.None);

        Assert.Null(result);
    }
}
