using Moq;
using WestcoastCars.Application.Features.Manufacturers.Queries.ListAll;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Queries;

public class ListAllManufacturersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IManufacturerRepository> _repositoryMock = new();
    private readonly ListAllManufacturersQueryHandler _handler;

    public ListAllManufacturersQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_repositoryMock.Object);
        _handler = new ListAllManufacturersQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos()
    {
        var manufacturers = new List<Manufacturer> { new() { Id = 1, Name = "Volvo" } };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(manufacturers);

        var result = await _handler.Handle(new ListAllManufacturersQuery(), CancellationToken.None);

        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(1, resultList[0].Id);
        Assert.Equal("Volvo", resultList[0].Name);
    }
}
