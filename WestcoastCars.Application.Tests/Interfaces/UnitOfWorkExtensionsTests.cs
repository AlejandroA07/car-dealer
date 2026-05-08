using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using Xunit;

namespace WestcoastCars.Application.Tests.Interfaces;

public class UnitOfWorkExtensionsTests
{
    [Fact]
    public async Task CompleteOrThrowAsync_ShouldReturn_WhenSaveAffectsRows()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(unitOfWork => unitOfWork.CompleteAsync()).ReturnsAsync(1);

        await unitOfWorkMock.Object.CompleteOrThrowAsync("Failed to save");

        unitOfWorkMock.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteOrThrowAsync_ShouldThrowPersistenceException_WhenSaveAffectsNoRows()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(unitOfWork => unitOfWork.CompleteAsync()).ReturnsAsync(0);

        var exception = await Assert.ThrowsAsync<PersistenceException>(() =>
            unitOfWorkMock.Object.CompleteOrThrowAsync("Failed to save"));

        Assert.Equal("Failed to save", exception.Message);
    }
}
