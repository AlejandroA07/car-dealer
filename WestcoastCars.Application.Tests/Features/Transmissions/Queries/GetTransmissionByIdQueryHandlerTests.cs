using Moq;
using WestcoastCars.Application.Features.Transmissions.Queries.GetById;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Transmissions.Queries;

public class GetTransmissionByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITransmissionTypeRepository> _repositoryMock = new();
    private readonly GetTransmissionByIdQueryHandler _handler;

    public GetTransmissionByIdQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_repositoryMock.Object);
        _handler = new GetTransmissionByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenTransmissionFound()
    {
        var transmission = new TransmissionType { Id = 1, Name = "Automatic" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transmission);

        var result = await _handler.Handle(new GetTransmissionByIdQuery { Id = 1 }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Automatic", result.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenTransmissionNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TransmissionType?)null);

        var result = await _handler.Handle(new GetTransmissionByIdQuery { Id = 99 }, CancellationToken.None);

        Assert.Null(result);
    }
}
