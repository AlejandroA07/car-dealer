using Moq;
using WestcoastCars.Application.Features.Vehicles.Commands.DeleteAll;
using WestcoastCars.Application.Interfaces;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Commands;

public class DeleteAllVehiclesCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly DeleteAllVehiclesCommandHandler _handler;

    public DeleteAllVehiclesCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new DeleteAllVehiclesCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDeletedCount_WhenVehiclesExist()
    {
        _repositoryMock.Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var result = await _handler.Handle(new DeleteAllVehiclesCommand(), CancellationToken.None);

        Assert.Equal(3, result.TotalDeleted);
        _repositoryMock.Verify(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnZero_WhenNoVehiclesExist()
    {
        _repositoryMock.Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _handler.Handle(new DeleteAllVehiclesCommand(), CancellationToken.None);

        Assert.Equal(0, result.TotalDeleted);
        _repositoryMock.Verify(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
