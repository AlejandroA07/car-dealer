using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.FuelTypes.Queries.ListAll;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.FuelTypes.Queries;

public class ListAllFuelTypesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFuelTypeRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ListAllFuelTypesQueryHandler _handler;

    public ListAllFuelTypesQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.FuelTypeRepository).Returns(_repositoryMock.Object);
        _handler = new ListAllFuelTypesQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos()
    {
        var fuelTypes = new List<FuelType> { new() { Id = 1, Name = "Diesel" } };
        var dtos = new List<NamedObjectDto> { new() { Id = 1, Name = "Diesel" } };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(fuelTypes);
        _mapperMock.Setup(m => m.Map<IEnumerable<NamedObjectDto>>(fuelTypes)).Returns(dtos);

        var result = await _handler.Handle(new ListAllFuelTypesQuery(), CancellationToken.None);

        Assert.Same(dtos, result);
    }
}
