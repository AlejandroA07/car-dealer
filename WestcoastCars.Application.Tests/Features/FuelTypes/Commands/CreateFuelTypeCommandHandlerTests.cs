using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.FuelTypes.Commands.Create;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.FuelTypes.Commands;

public class CreateFuelTypeCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IFuelTypeRepository> _repositoryMock = new();
    private readonly CreateFuelTypeCommandHandler _handler;

    public CreateFuelTypeCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.FuelTypeRepository).Returns(_repositoryMock.Object);
        _handler = new CreateFuelTypeCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateFuelType_AndReturnDto()
    {
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(new CreateFuelTypeCommand { Name = "Diesel" }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Diesel", result.Name);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<FuelType>(f => f.Name == "Diesel")), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenSaveFails()
    {
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        await Assert.ThrowsAsync<PersistenceException>(() =>
            _handler.Handle(new CreateFuelTypeCommand { Name = "Diesel" }, CancellationToken.None));
    }
}
