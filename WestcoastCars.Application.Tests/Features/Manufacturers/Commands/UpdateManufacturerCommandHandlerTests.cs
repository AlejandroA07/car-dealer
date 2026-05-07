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
    private readonly Mock<IManufacturerRepository> _manufacturerRepositoryMock;
    private readonly UpdateManufacturerCommandHandler _handler;

    public UpdateManufacturerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _manufacturerRepositoryMock = new Mock<IManufacturerRepository>();
        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_manufacturerRepositoryMock.Object);
        _handler = new UpdateManufacturerCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateManufacturer_WhenUpdateIsSuccessful()
    {
        var manufacturerId = 1;
        var command = new UpdateManufacturerCommand { Id = manufacturerId, Name = "Updated Name" };
        var existingManufacturer = new Manufacturer { Id = manufacturerId, Name = "Old Name" };

        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync(existingManufacturer);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(command.Name, existingManufacturer.Name);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        _unitOfWorkMock.VerifyGet(u => u.ManufacturerRepository, Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenSaveAffectsNoRows()
    {
        var manufacturerId = 1;
        var command = new UpdateManufacturerCommand { Id = manufacturerId, Name = "Updated Name" };
        var existingManufacturer = new Manufacturer { Id = manufacturerId, Name = "Old Name" };

        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(manufacturerId)).ReturnsAsync(existingManufacturer);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        await Assert.ThrowsAsync<PersistenceException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenManufacturerDoesNotExist()
    {
        var command = new UpdateManufacturerCommand { Id = 99, Name = "Test" };
        _manufacturerRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Manufacturer?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
