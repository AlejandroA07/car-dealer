using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.Manufacturers.Queries.GetById;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Queries;

public class GetManufacturerByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IManufacturerRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GetManufacturerByIdQueryHandler _handler;

    public GetManufacturerByIdQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_repositoryMock.Object);
        _handler = new GetManufacturerByIdQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenManufacturerFound()
    {
        var manufacturer = new Manufacturer { Id = 1, Name = "Volvo" };
        var dto = new NamedObjectDto { Id = 1, Name = "Volvo" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(manufacturer);
        _mapperMock.Setup(m => m.Map<NamedObjectDto>(manufacturer)).Returns(dto);

        var result = await _handler.Handle(new GetManufacturerByIdQuery { Id = 1 }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(dto, result);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenManufacturerNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Manufacturer?)null);

        var result = await _handler.Handle(new GetManufacturerByIdQuery { Id = 99 }, CancellationToken.None);

        Assert.Null(result);
    }
}
