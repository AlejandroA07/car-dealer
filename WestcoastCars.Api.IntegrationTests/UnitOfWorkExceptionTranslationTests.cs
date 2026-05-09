using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Repositories;

namespace WestcoastCars.Api.IntegrationTests;

public class UnitOfWorkExceptionTranslationTests : IntegrationTestBase
{
    public UnitOfWorkExceptionTranslationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CompleteAsync_ShouldTranslateUniqueVehicleRegistrationViolation_ToConflictException()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();
        var unitOfWork = new UnitOfWork(context, new PostgreSqlVehicleTextSearchMatcher());
        var (manufacturerId, fuelTypeId, transmissionTypeId) = await GetLookupIdsAsync(context);
        var registrationNumber = $"DBDUP{Guid.NewGuid():N}"[..10].ToUpperInvariant();

        context.Vehicles.Add(CreateVehicle(registrationNumber, manufacturerId, fuelTypeId, transmissionTypeId));
        await unitOfWork.CompleteAsync();

        context.Vehicles.Add(CreateVehicle(registrationNumber, manufacturerId, fuelTypeId, transmissionTypeId));

        var exception = await Assert.ThrowsAsync<ConflictException>(() => unitOfWork.CompleteAsync());

        exception.Message.Should().Be("Vehicle with the same registration number already exists.");
    }

    [Fact]
    public async Task CompleteAsync_ShouldTranslateMissingManufacturerForeignKey_ToNotFoundException()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();
        var unitOfWork = new UnitOfWork(context, new PostgreSqlVehicleTextSearchMatcher());
        var (_, fuelTypeId, transmissionTypeId) = await GetLookupIdsAsync(context);

        context.Vehicles.Add(CreateVehicle(
            registrationNumber: $"DBMFG{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            manufacturerId: int.MaxValue,
            fuelTypeId: fuelTypeId,
            transmissionTypeId: transmissionTypeId));

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => unitOfWork.CompleteAsync());

        exception.Message.Should().Be("Manufacturer no longer exists.");
    }

    [Fact]
    public async Task CompleteAsync_ShouldTranslateMissingFuelTypeForeignKey_ToNotFoundException()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();
        var unitOfWork = new UnitOfWork(context, new PostgreSqlVehicleTextSearchMatcher());
        var (manufacturerId, _, transmissionTypeId) = await GetLookupIdsAsync(context);

        context.Vehicles.Add(CreateVehicle(
            registrationNumber: $"DBFUEL{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            manufacturerId: manufacturerId,
            fuelTypeId: int.MaxValue,
            transmissionTypeId: transmissionTypeId));

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => unitOfWork.CompleteAsync());

        exception.Message.Should().Be("Fuel type no longer exists.");
    }

    [Fact]
    public async Task CompleteAsync_ShouldTranslateMissingTransmissionForeignKey_ToNotFoundException()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();
        var unitOfWork = new UnitOfWork(context, new PostgreSqlVehicleTextSearchMatcher());
        var (manufacturerId, fuelTypeId, _) = await GetLookupIdsAsync(context);

        context.Vehicles.Add(CreateVehicle(
            registrationNumber: $"DBTRNS{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            manufacturerId: manufacturerId,
            fuelTypeId: fuelTypeId,
            transmissionTypeId: int.MaxValue));

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => unitOfWork.CompleteAsync());

        exception.Message.Should().Be("Transmission type no longer exists.");
    }

    [Fact]
    public async Task CompleteAsync_ShouldTranslateUnknownForeignKeyViolation_ToConflictException()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();
        var unitOfWork = new UnitOfWork(context, new PostgreSqlVehicleTextSearchMatcher());

        context.Set<IdentityUserRole<string>>().Add(new IdentityUserRole<string>
        {
            UserId = "missing-user",
            RoleId = "missing-role"
        });

        var exception = await Assert.ThrowsAsync<ConflictException>(() => unitOfWork.CompleteAsync());

        exception.Message.Should().Be("The operation conflicts with existing related data.");
    }

    private static async Task<(int ManufacturerId, int FuelTypeId, int TransmissionTypeId)> GetLookupIdsAsync(WestcoastCarsContext context)
    {
        var manufacturerId = await context.Manufacturers.Select(manufacturer => manufacturer.Id).FirstAsync();
        var fuelTypeId = await context.FuelTypes.Select(fuelType => fuelType.Id).FirstAsync();
        var transmissionTypeId = await context.TransmissionTypes.Select(transmissionType => transmissionType.Id).FirstAsync();
        return (manufacturerId, fuelTypeId, transmissionTypeId);
    }

    private static Vehicle CreateVehicle(string registrationNumber, int manufacturerId, int fuelTypeId, int transmissionTypeId) =>
        new()
        {
            RegistrationNumber = registrationNumber,
            Model = "Constraint Test",
            ModelYear = 2024,
            Mileage = 100,
            ImageUrl = "/images/no-car.png",
            Price = 500000,
            Description = "Constraint translation test vehicle",
            ManufacturerId = manufacturerId,
            FuelTypeId = fuelTypeId,
            TransmissionTypeId = transmissionTypeId,
            Manufacturer = null!,
            FuelType = null!,
            TransmissionType = null!
        };
}
