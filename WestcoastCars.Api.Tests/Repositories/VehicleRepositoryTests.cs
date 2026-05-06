using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Repositories;
using Xunit;

namespace WestcoastCars.Api.Tests.Repositories;

public class VehicleRepositoryTests
{
    [Theory]
    [InlineData(nameof(Repository<Vehicle>.GetByIdAsync))]
    [InlineData(nameof(Repository<Vehicle>.GetAllAsync))]
    public void VehicleRepository_ShouldOverrideEagerLoadingMethods(string methodName)
    {
        // Arrange
        var method = typeof(VehicleRepository).GetMethod(methodName);

        // Act
        var baseDefinition = method?.GetBaseDefinition();

        // Assert
        Assert.NotNull(baseDefinition);
        Assert.Equal(typeof(Repository<Vehicle>), baseDefinition.DeclaringType);
        Assert.NotEqual(method, baseDefinition);
    }
}
