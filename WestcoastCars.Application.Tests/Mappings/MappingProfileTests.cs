using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;
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

    private static Vehicle CreateVehicle(string? imageUrl) =>
        new()
        {
            Id = 1,
            RegistrationNumber = "ABC123",
            Model = "V60",
            ModelYear = "2024",
            Mileage = 1000,
            ImageUrl = imageUrl!,
            Value = 450000,
            Description = "Test vehicle",
            Manufacturer = new Manufacturer { Id = 1, Name = "Volvo" },
            FuelType = new FuelType { Id = 1, Name = "Diesel" },
            TransmissionType = new TransmissionType { Id = 1, Name = "Automatic" }
        };
}
