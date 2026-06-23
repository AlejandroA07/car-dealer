using Moq;
using WestcoastCars.Application.Features.Manufacturers.Queries.GetById;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Queries;

public class GetManufacturerByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IManufacturerRepository> _repositoryMock = new();
    private readonly GetManufacturerByIdQueryHandler _handler;

    public GetManufacturerByIdQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_repositoryMock.Object);
        _handler = new GetManufacturerByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenManufacturerFound()
    {
        var manufacturer = new Manufacturer { Id = 1, Name = "Volvo" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(manufacturer);

        var result = await _handler.Handle(new GetManufacturerByIdQuery { Id = 1 }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Volvo", result.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenManufacturerNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Manufacturer?)null);

        var result = await _handler.Handle(new GetManufacturerByIdQuery { Id = 99 }, CancellationToken.None);

        Assert.Null(result);
    }
}
