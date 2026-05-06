using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Interfaces;

public class RepositoryExtensionsTests
{
    private readonly Mock<IManufacturerRepository> _repositoryMock = new();

    [Fact]
    public async Task ThrowIfNameExistsAsync_ShouldThrowConflictException_WhenNameExists()
    {
        // Arrange
        _repositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>()))
            .ReturnsAsync(new Manufacturer { Id = 1, Name = "Volvo" });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _repositoryMock.Object.ThrowIfNameExistsAsync("Volvo", nameof(Manufacturer)));
    }

    [Fact]
    public async Task ThrowIfNameExistsAsync_ShouldNotThrow_WhenExistingEntityMatchesExcludedId()
    {
        // Arrange
        _repositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>()))
            .ReturnsAsync(new Manufacturer { Id = 1, Name = "Volvo" });

        // Act
        await _repositoryMock.Object.ThrowIfNameExistsAsync("Volvo", nameof(Manufacturer), excludingId: 1);
    }

    [Fact]
    public async Task ThrowIfNameExistsAsync_ShouldNotThrow_WhenNameIsUnique()
    {
        // Arrange
        _repositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>()))
            .ReturnsAsync((Manufacturer?)null);

        // Act
        await _repositoryMock.Object.ThrowIfNameExistsAsync("Volvo", nameof(Manufacturer));
    }
}
