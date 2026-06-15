using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.Transmissions.Queries.GetById;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Transmissions.Queries;

public class GetTransmissionByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITransmissionTypeRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GetTransmissionByIdQueryHandler _handler;

    public GetTransmissionByIdQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_repositoryMock.Object);
        _handler = new GetTransmissionByIdQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenTransmissionFound()
    {
        var transmission = new TransmissionType { Id = 1, Name = "Automatic" };
        var dto = new NamedObjectDto { Id = 1, Name = "Automatic" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transmission);
        _mapperMock.Setup(m => m.Map<NamedObjectDto>(transmission)).Returns(dto);

        var result = await _handler.Handle(new GetTransmissionByIdQuery { Id = 1 }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(dto, result);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTransmissionNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TransmissionType?)null);

        var result = await _handler.Handle(new GetTransmissionByIdQuery { Id = 99 }, CancellationToken.None);

        Assert.Null(result);
    }
}
