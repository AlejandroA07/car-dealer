using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Manufacturers.Commands.Update;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Commands;

public class UpdateManufacturerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Manufacturer>> _manufacturerRepositoryMock;
    private readonly UpdateManufacturerCommandHandler _handler;

    public UpdateManufacturerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _manufacturerRepositoryMock = new Mock<IRepository<Manufacturer>>();
        _unitOfWorkMock.Setup(u => u.Repository<Manufacturer>()).Returns(_manufacturerRepositoryMock.Object);
        _handler = new UpdateManufacturerCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateManufacturer_WhenUpdateIsSuccessful()
    {
        // Arrange
        var manufacturerId = 1;
        var command = new UpdateManufacturerCommand { Id = manufacturerId, Name = "Updated Name" };
        var existingManufacturer = new Manufacturer { Id = manufacturerId, Name = "Old Name" };

        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync(existingManufacturer);
        _manufacturerRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>()))
            .ReturnsAsync((Manufacturer?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(command.Name, existingManufacturer.Name);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenManufacturerDoesNotExist()
    {
        // Arrange
        var command = new UpdateManufacturerCommand { Id = 99, Name = "Test" };
        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Manufacturer?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenNameAlreadyExists()
    {
        // Arrange
        var manufacturerId = 1;
        var command = new UpdateManufacturerCommand { Id = manufacturerId, Name = "Existing Name" };
        var existingManufacturer = new Manufacturer { Id = manufacturerId, Name = "Old Name" };
        var otherManufacturer = new Manufacturer { Id = 2, Name = "Existing Name" };

        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync(existingManufacturer);
        _manufacturerRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>()))
            .ReturnsAsync(otherManufacturer);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
