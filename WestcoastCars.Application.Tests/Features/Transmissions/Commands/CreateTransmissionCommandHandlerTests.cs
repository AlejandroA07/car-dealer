using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Transmissions.Commands.Create;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Transmissions.Commands;

public class CreateTransmissionCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITransmissionTypeRepository> _repositoryMock = new();
    private readonly CreateTransmissionCommandHandler _handler;

    public CreateTransmissionCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_repositoryMock.Object);
        _handler = new CreateTransmissionCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTransmission_AndReturnDto()
    {
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(
            new CreateTransmissionCommand { Name = "Automatic" }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Automatic", result.Name);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<TransmissionType>(t => t.Name == "Automatic")), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenSaveFails()
    {
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        await Assert.ThrowsAsync<PersistenceException>(() =>
            _handler.Handle(new CreateTransmissionCommand { Name = "Automatic" }, CancellationToken.None));
    }
}
