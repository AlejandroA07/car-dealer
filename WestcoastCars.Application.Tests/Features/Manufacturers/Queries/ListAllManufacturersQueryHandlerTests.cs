using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.Manufacturers.Queries.ListAll;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Queries;

public class ListAllManufacturersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IManufacturerRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ListAllManufacturersQueryHandler _handler;

    public ListAllManufacturersQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_repositoryMock.Object);
        _handler = new ListAllManufacturersQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos()
    {
        var manufacturers = new List<Manufacturer> { new() { Id = 1, Name = "Volvo" } };
        var dtos = new List<NamedObjectDto> { new() { Id = 1, Name = "Volvo" } };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(manufacturers);
        _mapperMock.Setup(m => m.Map<IEnumerable<NamedObjectDto>>(manufacturers)).Returns(dtos);

        var result = await _handler.Handle(new ListAllManufacturersQuery(), CancellationToken.None);

        Assert.Same(dtos, result);
    }
}
