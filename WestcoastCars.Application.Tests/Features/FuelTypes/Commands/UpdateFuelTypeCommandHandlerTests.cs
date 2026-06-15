using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.FuelTypes.Commands.Update;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.FuelTypes.Commands;

public class UpdateFuelTypeCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFuelTypeRepository> _repositoryMock = new();
    private readonly UpdateFuelTypeCommandHandler _handler;

    public UpdateFuelTypeCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.FuelTypeRepository).Returns(_repositoryMock.Object);
        _handler = new UpdateFuelTypeCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateFuelType_WhenFound()
    {
        var fuelType = new FuelType { Id = 1, Name = "Petrol" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fuelType);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        await _handler.Handle(new UpdateFuelTypeCommand { Id = 1, Name = "Diesel" }, CancellationToken.None);

        Assert.Equal("Diesel", fuelType.Name);
        _repositoryMock.Verify(r => r.Update(fuelType), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenFuelTypeNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FuelType?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new UpdateFuelTypeCommand { Id = 99, Name = "X" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenSaveFails()
    {
        var fuelType = new FuelType { Id = 1, Name = "Petrol" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fuelType);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        await Assert.ThrowsAsync<PersistenceException>(() =>
            _handler.Handle(new UpdateFuelTypeCommand { Id = 1, Name = "Diesel" }, CancellationToken.None));
    }
}
