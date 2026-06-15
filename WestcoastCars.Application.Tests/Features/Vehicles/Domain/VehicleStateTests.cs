using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Domain;

public class VehicleStateTests
{
    private static Vehicle CreateVehicle() => new()
    {
        RegistrationNumber = "TEST123",
        Model = "Test",
        ModelYear = 2020,
        ImageUrl = "test.png",
        Description = "Test",
        Manufacturer = new Manufacturer { Name = "Make" },
        FuelType = new FuelType { Name = "Fuel" },
        TransmissionType = new TransmissionType { Name = "Trans" }
    };

    [Fact]
    public void MarkAsSold_ShouldSetIsSoldToTrue()
    {
        var vehicle = CreateVehicle();
        vehicle.MarkAsSold();
        Assert.True(vehicle.IsSold);
    }

    [Fact]
    public void MarkAsSold_ShouldThrow_WhenAlreadySold()
    {
        var vehicle = CreateVehicle();
        vehicle.MarkAsSold();
        Assert.Throws<InvalidOperationException>(() => vehicle.MarkAsSold());
    }

    [Fact]
    public void MarkAsAvailable_ShouldSetIsSoldToFalse()
    {
        var vehicle = CreateVehicle();
        vehicle.MarkAsSold();
        vehicle.MarkAsAvailable();
        Assert.False(vehicle.IsSold);
    }

    [Fact]
    public void MarkAsAvailable_ShouldBeIdempotent_WhenAlreadyAvailable()
    {
        var vehicle = CreateVehicle();
        vehicle.MarkAsAvailable();
        Assert.False(vehicle.IsSold);
    }

    [Fact]
    public void MarkAsSourceRemoved_ShouldSetStatusAndTimestamp()
    {
        var vehicle = CreateVehicle();
        var removedAt = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        vehicle.MarkAsSourceRemoved(removedAt);

        Assert.Equal("SourceRemoved", vehicle.SourceStatus);
        Assert.Equal(removedAt, vehicle.SourceRemovedAt);
    }
}
