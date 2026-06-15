using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Transmissions.Commands.Update;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Transmissions.Commands;

public class UpdateTransmissionCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITransmissionTypeRepository> _repositoryMock = new();
    private readonly UpdateTransmissionCommandHandler _handler;

    public UpdateTransmissionCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_repositoryMock.Object);
        _handler = new UpdateTransmissionCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTransmission_WhenFound()
    {
        var transmission = new TransmissionType { Id = 1, Name = "Manual" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transmission);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        await _handler.Handle(new UpdateTransmissionCommand { Id = 1, Name = "Automatic" }, CancellationToken.None);

        Assert.Equal("Automatic", transmission.Name);
        _repositoryMock.Verify(r => r.Update(transmission), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTransmissionNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TransmissionType?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new UpdateTransmissionCommand { Id = 99, Name = "X" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenSaveFails()
    {
        var transmission = new TransmissionType { Id = 1, Name = "Manual" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transmission);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        await Assert.ThrowsAsync<PersistenceException>(() =>
            _handler.Handle(new UpdateTransmissionCommand { Id = 1, Name = "Automatic" }, CancellationToken.None));
    }
}
