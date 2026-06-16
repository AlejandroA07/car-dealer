using Moq;
using WestcoastCars.Application.Features.Vehicles.Commands.PurgeSourceRemoved;
using WestcoastCars.Application.Interfaces;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class PurgeSourceRemovedVehiclesCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly PurgeSourceRemovedVehiclesCommandHandler _handler;

    public PurgeSourceRemovedVehiclesCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new PurgeSourceRemovedVehiclesCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDeletedCount_WhenSourceRemovedVehiclesExist()
    {
        _repositoryMock.Setup(r => r.PurgeSourceRemovedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var result = await _handler.Handle(new PurgeSourceRemovedVehiclesCommand(), CancellationToken.None);

        Assert.Equal(2, result.TotalDeleted);
        _repositoryMock.Verify(r => r.PurgeSourceRemovedAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnZero_WhenNoSourceRemovedVehiclesExist()
    {
        _repositoryMock.Setup(r => r.PurgeSourceRemovedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _handler.Handle(new PurgeSourceRemovedVehiclesCommand(), CancellationToken.None);

        Assert.Equal(0, result.TotalDeleted);
        _repositoryMock.Verify(r => r.PurgeSourceRemovedAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
