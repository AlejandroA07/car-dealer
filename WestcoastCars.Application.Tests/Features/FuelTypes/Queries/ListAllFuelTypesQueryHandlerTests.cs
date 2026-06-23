using Moq;
using WestcoastCars.Application.Features.FuelTypes.Queries.ListAll;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.FuelTypes.Queries;

public class ListAllFuelTypesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFuelTypeRepository> _repositoryMock = new();
    private readonly ListAllFuelTypesQueryHandler _handler;

    public ListAllFuelTypesQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.FuelTypeRepository).Returns(_repositoryMock.Object);
        _handler = new ListAllFuelTypesQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos()
    {
        var fuelTypes = new List<FuelType> { new() { Id = 1, Name = "Diesel" } };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(fuelTypes);

        var result = await _handler.Handle(new ListAllFuelTypesQuery(), CancellationToken.None);

        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(1, resultList[0].Id);
        Assert.Equal("Diesel", resultList[0].Name);
    }
}
