using Moq;
using WestcoastCars.Application.Features.Vehicles.Commands.BulkDelete;
using WestcoastCars.Application.Interfaces;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class BulkDeleteVehiclesCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly BulkDeleteVehiclesCommandHandler _handler;

    public BulkDeleteVehiclesCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new BulkDeleteVehiclesCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenNoFiltersSpecified()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(new BulkDeleteVehiclesCommand(), CancellationToken.None));

        _repositoryMock.Verify(r => r.BulkDeleteAsync(
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnDeletedCount_WhenMatchingVehiclesFound()
    {
        _repositoryMock
            .Setup(r => r.BulkDeleteAsync("Volvo", null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await _handler.Handle(
            new BulkDeleteVehiclesCommand { Make = "Volvo" }, CancellationToken.None);

        Assert.Equal(2, result.TotalDeleted);
        _repositoryMock.Verify(r => r.BulkDeleteAsync("Volvo", null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnZero_WhenNoMatchingVehiclesFound()
    {
        _repositoryMock
            .Setup(r => r.BulkDeleteAsync(null, "XC60", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.Handle(
            new BulkDeleteVehiclesCommand { Model = "XC60" }, CancellationToken.None);

        Assert.Equal(0, result.TotalDeleted);
        _repositoryMock.Verify(r => r.BulkDeleteAsync(null, "XC60", null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
