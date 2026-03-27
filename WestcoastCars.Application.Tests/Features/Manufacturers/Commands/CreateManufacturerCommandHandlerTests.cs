using AutoMapper;
using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Manufacturers.Commands.Create;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Manufacturers.Commands;

public class CreateManufacturerCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Manufacturer>> _manufacturerRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateManufacturerCommandHandler _handler;

    public CreateManufacturerCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _manufacturerRepositoryMock = new Mock<IRepository<Manufacturer>>();
        _mapperMock = new Mock<IMapper>();

        _unitOfWorkMock.Setup(u => u.Repository<Manufacturer>()).Returns(_manufacturerRepositoryMock.Object);

        _handler = new CreateManufacturerCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateManufacturer_WhenNameIsUnique()
    {
        // Arrange
        var command = new CreateManufacturerCommand { Name = "Volvo" };
        var manufacturer = new Manufacturer { Id = 1, Name = "Volvo" };
        var expectedDto = new NamedObjectDto { Id = 1, Name = "Volvo" };

        _manufacturerRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>()))
            .ReturnsAsync((Manufacturer?)null);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<NamedObjectDto>(It.IsAny<Manufacturer>())).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Name, result.Name);
        _manufacturerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Manufacturer>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenManufacturerExists()
    {
        // Arrange
        var command = new CreateManufacturerCommand { Name = "Volvo" };
        var existingManufacturer = new Manufacturer { Id = 1, Name = "Volvo" };

        _manufacturerRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>()))
            .ReturnsAsync(existingManufacturer);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
