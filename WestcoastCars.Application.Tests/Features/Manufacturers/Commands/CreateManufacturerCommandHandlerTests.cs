using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Manufacturers.Commands.Create;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Commands;

public class CreateManufacturerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IManufacturerRepository> _manufacturerRepositoryMock;
    private readonly CreateManufacturerCommandHandler _handler;

    public CreateManufacturerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _manufacturerRepositoryMock = new Mock<IManufacturerRepository>();

        _unitOfWorkMock.Setup(u => u.ManufacturerRepository).Returns(_manufacturerRepositoryMock.Object);

        _handler = new CreateManufacturerCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateManufacturer_WhenNameIsUnique()
    {
        // Arrange
        var command = new CreateManufacturerCommand { Name = "Volvo" };

        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Volvo", result.Name);
        _manufacturerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Manufacturer>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenSaveAffectsNoRows()
    {
        // Arrange
        var command = new CreateManufacturerCommand { Name = "Volvo" };

        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        // Act & Assert
        await Assert.ThrowsAsync<PersistenceException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
