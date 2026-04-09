using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Application.Services;
using Xunit;

namespace WestcoastCars.Application.Tests.Services;

public class BlocketVehicleImportMapperTests
{
    private readonly BlocketVehicleImportMapper _mapper = new();

    [Fact]
    public void Map_ShouldNormalizeSwedishFields_AndConvertMileageToKilometers()
    {
        var importedAt = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc);
        var searchItem = new BlocketCarSearchItem
        {
            Id = "22221687",
            RegistrationNumber = "ONO054",
            Make = "Volvo",
            Model = "XC60",
            Heading = "Volvo XC60",
            Location = "Göteborg",
            Timestamp = 1775736691638,
            CanonicalUrl = "https://www.blocket.se/mobility/item/22221687",
            Mileage = 8107,
            MileageUnit = "SCANDINAVIAN_MILE",
            Transmission = "Automatisk",
            Fuel = "Bensin",
            ModelSpecification = "T5 250hk Momentum",
            Price = new BlocketPrice { Amount = 319900 }
        };

        var adDetails = new BlocketCarAdDetails
        {
            Url = "https://www.blocket.se/mobility/item/22221687",
            Title = "Volvo XC60",
            Subtitle = "T5 250hk Momentum Keyless",
            Specifications = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Färg"] = "Vit",
                ["Registreringsnummer"] = "ONO054"
            }
        };

        var result = _mapper.Map(searchItem, adDetails, importedAt);

        Assert.Equal("ONO054", result.RegistrationNumber);
        Assert.Equal("Volvo", result.Manufacturer);
        Assert.Equal("XC60", result.Model);
        Assert.Equal(81070, result.Mileage);
        Assert.Equal("Petrol", result.FuelType);
        Assert.Equal("Automatic", result.TransmissionType);
        Assert.Equal("Vit", result.Color);
        Assert.Equal("Göteborg", result.City);
        Assert.Equal(319900, result.Value);
        Assert.Equal("/images/no-car.png", result.ImageUrl);
        Assert.Equal(importedAt, result.ImportedAt);
        Assert.Equal("Blocket", result.Source);
        Assert.NotNull(result.PublishedAt);
    }

    [Fact]
    public void Map_ShouldUseDetailsSpecifications_WhenSearchFieldsAreMissing()
    {
        var searchItem = new BlocketCarSearchItem
        {
            Id = "123",
            Heading = "Audi A4",
            Timestamp = 1775736691638
        };

        var adDetails = new BlocketCarAdDetails
        {
            Title = "Audi A4",
            Subtitle = "2.0 TDI",
            ModelYear = "2020",
            Mileage = "8 107 mil",
            Fuel = "Diesel",
            Transmission = "Manuell",
            Price = "319 900 kr",
            Specifications = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Märke"] = "Audi",
                ["Modell"] = "A4",
                ["Registreringsnummer"] = "ABC123",
                ["Färg"] = "Svart"
            }
        };

        var result = _mapper.Map(searchItem, adDetails, DateTime.UtcNow);

        Assert.Equal("Audi", result.Manufacturer);
        Assert.Equal("A4", result.Model);
        Assert.Equal("2020", result.ModelYear);
        Assert.Equal(81070, result.Mileage);
        Assert.Equal("Diesel", result.FuelType);
        Assert.Equal("Manual", result.TransmissionType);
        Assert.Equal("ABC123", result.RegistrationNumber);
        Assert.Equal(319900, result.Value);
        Assert.Equal("2.0 TDI", result.Description);
    }

    [Fact]
    public void Map_ShouldAllowMissingRegistrationNumber_AndFallbackToDefaults()
    {
        var searchItem = new BlocketCarSearchItem
        {
            Id = "456",
            Make = "Tesla",
            Model = "Model 3",
            Heading = "Tesla Model 3",
            Timestamp = 0,
            Fuel = null,
            Transmission = null,
            ModelSpecification = null,
            Price = null
        };

        var adDetails = new BlocketCarAdDetails
        {
            Title = "Tesla Model 3",
            Subtitle = null,
            Specifications = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };

        var result = _mapper.Map(searchItem, adDetails, DateTime.UtcNow);

        Assert.Null(result.RegistrationNumber);
        Assert.Equal("Unknown", result.FuelType);
        Assert.Equal("Unknown", result.TransmissionType);
        Assert.Equal("/images/no-car.png", result.ImageUrl);
        Assert.Equal(0, result.Value);
        Assert.Equal("Tesla Model 3", result.Description);
        Assert.Null(result.PublishedAt);
    }
}
