using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Manufacturers.Commands.Delete;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Commands;

public class DeleteManufacturerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Manufacturer>> _manufacturerRepositoryMock;
    private readonly DeleteManufacturerCommandHandler _handler;

    public DeleteManufacturerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _manufacturerRepositoryMock = new Mock<IRepository<Manufacturer>>();
        _unitOfWorkMock.Setup(u => u.Repository<Manufacturer>()).Returns(_manufacturerRepositoryMock.Object);
        _handler = new DeleteManufacturerCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteManufacturer_WhenManufacturerExists()
    {
        // Arrange
        var manufacturerId = 1;
        var manufacturer = new Manufacturer { Id = manufacturerId, Name = "Volvo" };

        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync(manufacturer);

        // Act
        await _handler.Handle(new DeleteManufacturerCommand { Id = manufacturerId }, CancellationToken.None);

        // Assert
        _manufacturerRepositoryMock.Verify(r => r.Remove(manufacturer), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenManufacturerDoesNotExist()
    {
        // Arrange
        var manufacturerId = 99;
        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync((Manufacturer?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new DeleteManufacturerCommand { Id = manufacturerId }, CancellationToken.None));
    }
}
