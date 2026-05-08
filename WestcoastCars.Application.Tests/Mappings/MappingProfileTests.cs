using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        _mapper = configuration.CreateMapper();
    }

    [Theory]
    [InlineData(null, "/images/no-car.png")]
    [InlineData("", "/images/no-car.png")]
    [InlineData("no-car.png", "/images/no-car.png")]
    [InlineData("car.png", "/images/car.png")]
    [InlineData("/images/car.png", "/images/car.png")]
    [InlineData("https://example.com/car.png", "https://example.com/car.png")]
    public void VehicleMappings_ShouldNormalizeImageUrl(string? imageUrl, string expectedImageUrl)
    {
        // Arrange
        var vehicle = CreateVehicle(imageUrl);

        // Act
        var summary = _mapper.Map<VehicleSummaryDto>(vehicle);
        var details = _mapper.Map<VehicleDetailsDto>(vehicle);

        // Assert
        Assert.Equal(expectedImageUrl, summary.ImageUrl);
        Assert.Equal(expectedImageUrl, details.ImageUrl);
        Assert.Equal("Automatic", details.TransmissionType);
    }

    [Fact]
    public void VehicleSummaryMapping_ShouldMapExpectedFields()
    {
        var publishedAt = new DateTime(2026, 5, 8, 10, 30, 0, DateTimeKind.Utc);
        var vehicle = CreateVehicle("xc60.png");
        vehicle.IsSold = true;
        vehicle.Price = 525000;
        vehicle.Color = "Blue";
        vehicle.City = "Gothenburg";
        vehicle.Source = "Blocket";
        vehicle.PublishedAt = publishedAt;

        var summary = _mapper.Map<VehicleSummaryDto>(vehicle);

        Assert.Equal(vehicle.Id, summary.Id);
        Assert.Equal("Volvo XC60", summary.Name);
        Assert.Equal(vehicle.Manufacturer.Name, summary.Manufacturer);
        Assert.Equal(vehicle.Model, summary.Model);
        Assert.Equal(vehicle.ModelYear, summary.ModelYear);
        Assert.Equal("/images/xc60.png", summary.ImageUrl);
        Assert.Equal(vehicle.IsSold, summary.IsSold);
        Assert.Equal(525000m, summary.Price);
        Assert.Equal(vehicle.Color, summary.Color);
        Assert.Equal(vehicle.City, summary.City);
        Assert.Equal(vehicle.Source, summary.Source);
        Assert.Equal(publishedAt, summary.PublishedAt);
    }

    [Fact]
    public void VehicleDetailsMapping_ShouldMapExpectedFields()
    {
        var publishedAt = new DateTime(2026, 5, 8, 10, 30, 0, DateTimeKind.Utc);
        var importedAt = new DateTime(2026, 5, 8, 11, 0, 0, DateTimeKind.Utc);
        var vehicle = CreateVehicle("https://example.com/xc60.png");
        vehicle.ExternalListingId = "12345";
        vehicle.Source = "Blocket";
        vehicle.SourceUrl = "https://www.blocket.se/ad/12345";
        vehicle.PublishedAt = publishedAt;
        vehicle.ImportedAt = importedAt;
        vehicle.Color = "Blue";
        vehicle.City = "Gothenburg";

        var details = _mapper.Map<VehicleDetailsDto>(vehicle);

        Assert.Equal(vehicle.Id, details.Id);
        Assert.Equal(vehicle.RegistrationNumber, details.RegistrationNumber);
        Assert.Equal(vehicle.FuelType.Name, details.FuelType);
        Assert.Equal(vehicle.TransmissionType.Name, details.TransmissionType);
        Assert.Equal(vehicle.Mileage, details.Mileage);
        Assert.Equal(vehicle.Price, details.Price);
        Assert.Equal(vehicle.Description, details.Description);
        Assert.Equal("Volvo XC60", details.Name);
        Assert.Equal(vehicle.Manufacturer.Name, details.Manufacturer);
        Assert.Equal(vehicle.Model, details.Model);
        Assert.Equal(vehicle.ModelYear, details.ModelYear);
        Assert.Equal("https://example.com/xc60.png", details.ImageUrl);
        Assert.Equal(vehicle.IsSold, details.IsSold);
        Assert.Equal(vehicle.ExternalListingId, details.ExternalListingId);
        Assert.Equal(vehicle.Source, details.Source);
        Assert.Equal(vehicle.SourceUrl, details.SourceUrl);
        Assert.Equal(publishedAt, details.PublishedAt);
        Assert.Equal(importedAt, details.ImportedAt);
        Assert.Equal(vehicle.Color, details.Color);
        Assert.Equal(vehicle.City, details.City);
    }

    [Fact]
    public void ServiceBookingSummaryMapping_ShouldMapExpectedFields()
    {
        var bookingDate = new DateTime(2026, 5, 15, 9, 0, 0, DateTimeKind.Utc);
        var createdAt = new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc);
        var booking = new ServiceBooking
        {
            Id = 7,
            VehicleRegistrationNumber = "ABC123",
            ServiceType = "Annual service",
            BookingDate = bookingDate,
            CustomerName = "Alex Customer",
            CustomerEmail = "alex@example.com",
            CustomerPhone = "0701234567",
            Description = "Oil change",
            Status = BookingStatus.Confirmed,
            CreatedAt = createdAt
        };

        var summary = _mapper.Map<ServiceBookingSummaryDto>(booking);

        Assert.Equal(booking.Id, summary.Id);
        Assert.Equal(booking.VehicleRegistrationNumber, summary.VehicleRegistrationNumber);
        Assert.Equal(booking.ServiceType, summary.ServiceType);
        Assert.Equal(booking.BookingDate, summary.BookingDate);
        Assert.Equal(booking.CustomerName, summary.CustomerName);
        Assert.Equal(booking.CustomerEmail, summary.CustomerEmail);
        Assert.Equal(booking.CustomerPhone, summary.CustomerPhone);
        Assert.Equal("Confirmed", summary.Status);
        Assert.Equal(booking.CreatedAt, summary.CreatedAt);
    }

    private static Vehicle CreateVehicle(string? imageUrl) =>
        new()
        {
            Id = 1,
            RegistrationNumber = "ABC123",
            Model = "XC60",
            ModelYear = 2024,
            Mileage = 1000,
            ImageUrl = imageUrl!,
            Price = 450000,
            Description = "Test vehicle",
            Manufacturer = new Manufacturer { Id = 1, Name = "Volvo" },
            FuelType = new FuelType { Id = 1, Name = "Diesel" },
            TransmissionType = new TransmissionType { Id = 1, Name = "Automatic" }
        };
}
