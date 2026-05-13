using System.Globalization;
using System.Text.RegularExpressions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;

namespace WestcoastCars.Application.Services;

public class BlocketVehicleImportMapper : IBlocketVehicleImportMapper
{
    private const string DefaultImageUrl = "/images/no-car.png";

    // Blocket uses these Swedish placeholders when data is missing — treat all as unknown
    private static readonly HashSet<string> BlocketUnknownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "okänd", "unknown", "ej angiven", "saknas", "övrigt"
    };

    private static readonly Dictionary<string, string> FuelMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Bensin"] = "Petrol",
        ["Diesel"] = "Diesel",
        ["El"] = "Electric",
        ["Elektrisk"] = "Electric",
        ["Bensin/El"] = "Petrol/Electric",
        ["Diesel/El"] = "Diesel/Electric",
        ["Hybrid"] = "Hybrid",
        ["Laddhybrid"] = "Plug-in Electric Hybrid",
        ["Etanol"] = "Ethanol",
        ["Gas"] = "Gas",
        ["Biogas"] = "Bio Gas",
        ["Vätgas"] = "Hydrogen"
    };

    private static readonly Dictionary<string, string> TransmissionMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Automatisk"] = "Automatic",
        ["Automat"] = "Automatic",
        ["Manuell"] = "Manual",
        ["Manuel"] = "Manual"
    };

    public BlocketVehicleImportData Map(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails, DateTime importedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(searchItem);

        return new BlocketVehicleImportData
        {
            RegistrationNumber = NormalizeOptional(searchItem.RegistrationNumber)
                ?? GetSpecification(adDetails, "Registreringsnummer"),
            Manufacturer = NormalizeManufacturer(searchItem.Make, adDetails),
            Model = NormalizeModel(searchItem.Model, searchItem.Heading, adDetails),
            ModelYear = NormalizeModelYear(searchItem.Year, adDetails),
            Mileage = NormalizeMileage(searchItem, adDetails),
            ImageUrl = NormalizeImageUrl(searchItem, adDetails),
            Price = NormalizePrice(searchItem, adDetails),
            Description = NormalizeDescription(searchItem, adDetails),
            FuelType = NormalizeFuelType(searchItem.Fuel, adDetails),
            TransmissionType = NormalizeTransmission(searchItem.Transmission, adDetails),
            ExternalListingId = searchItem.Id,
            Source = "Blocket",
            SourceUrl = NormalizeOptional(searchItem.CanonicalUrl)
                ?? NormalizeOptional(adDetails?.Url),
            PublishedAt = NormalizePublishedAt(searchItem.Timestamp),
            ImportedAt = importedAtUtc,
            Color = NormalizeColor(adDetails),
            City = NormalizeOptional(searchItem.Location)
        };
    }

    private static string NormalizeManufacturer(string? searchMake, BlocketCarAdDetails? adDetails)
    {
        var candidate = NormalizeOptional(searchMake);
        if (candidate is null || BlocketUnknownValues.Contains(candidate))
            candidate = GetSpecification(adDetails, "Märke");
        if (candidate is null || BlocketUnknownValues.Contains(candidate))
            return "Unknown";

        return candidate.Trim();
    }

    private static string NormalizeModel(string? model, string? heading, BlocketCarAdDetails? adDetails)
    {
        var normalizedModel = NormalizeOptional(model)
            ?? GetSpecification(adDetails, "Modell")
            ?? NormalizeOptional(heading)
            ?? NormalizeOptional(adDetails?.Title)
            ?? "Unknown";

        return normalizedModel.Trim();
    }

    private static int? NormalizeModelYear(int? year, BlocketCarAdDetails? adDetails)
    {
        if (year.HasValue && IsValidModelYear(year.Value))
        {
            return year.Value;
        }

        var detailsYear = NormalizeOptional(adDetails?.ModelYearText)
            ?? GetSpecification(adDetails, "Modellår");

        return ParseModelYear(detailsYear);
    }

    private static int NormalizeMileage(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails)
    {
        if (searchItem.Mileage.HasValue)
        {
            return string.Equals(searchItem.MileageUnit, "SCANDINAVIAN_MILE", StringComparison.OrdinalIgnoreCase)
                ? searchItem.Mileage.Value * 10
                : searchItem.Mileage.Value;
        }

        return ParseMileage(GetSpecification(adDetails, "Miltal"))
            ?? ParseMileage(adDetails?.Mileage)
            ?? 0;
    }

    private static string NormalizeImageUrl(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails)
    {
        var imageUrl = searchItem.Image?.Url
            ?? adDetails?.Image?.Url;

        return NormalizeOptional(imageUrl) ?? DefaultImageUrl;
    }

    private static int NormalizePrice(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails)
    {
        if (searchItem.Price?.Amount is int amount)
        {
            return amount;
        }

        return ParseInteger(adDetails?.Price) ?? 0;
    }

    private static string NormalizeDescription(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails)
    {
        return NormalizeOptional(searchItem.ModelSpecification)
            ?? NormalizeOptional(adDetails?.Subtitle)
            ?? NormalizeOptional(searchItem.Heading)
            ?? NormalizeOptional(adDetails?.Title)
            ?? string.Empty;
    }

    private static string NormalizeFuelType(string? fuel, BlocketCarAdDetails? adDetails)
    {
        var rawFuel = NormalizeOptional(fuel)
            ?? GetSpecification(adDetails, "Drivmedel")
            ?? NormalizeOptional(adDetails?.Fuel);

        if (string.IsNullOrWhiteSpace(rawFuel) || BlocketUnknownValues.Contains(rawFuel))
        {
            return "Unknown";
        }

        return FuelMappings.TryGetValue(rawFuel, out var normalizedFuel)
            ? normalizedFuel
            : rawFuel;
    }

    private static string NormalizeTransmission(string? transmission, BlocketCarAdDetails? adDetails)
    {
        var rawTransmission = NormalizeOptional(transmission)
            ?? GetSpecification(adDetails, "Växellåda")
            ?? NormalizeOptional(adDetails?.Transmission);

        if (string.IsNullOrWhiteSpace(rawTransmission) || BlocketUnknownValues.Contains(rawTransmission))
        {
            return "Unknown";
        }

        return TransmissionMappings.TryGetValue(rawTransmission, out var normalizedTransmission)
            ? normalizedTransmission
            : rawTransmission;
    }

    private static DateTime? NormalizePublishedAt(long timestamp)
    {
        if (timestamp <= 0)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
    }

    private static string? NormalizeColor(BlocketCarAdDetails? adDetails)
    {
        return GetSpecification(adDetails, "Färg")
            ?? GetSpecification(adDetails, "Färgbeskrivning");
    }

    private static string? GetSpecification(BlocketCarAdDetails? adDetails, string key)
    {
        if (adDetails?.Specifications is null)
        {
            return null;
        }

        return adDetails.Specifications.TryGetValue(key, out var value)
            ? NormalizeOptional(value)
            : null;
    }

    private static int? ParseMileage(string? value)
    {
        var numericValue = ParseInteger(value);
        if (!numericValue.HasValue)
        {
            return null;
        }

        return value?.Contains("mil", StringComparison.OrdinalIgnoreCase) == true
            ? numericValue.Value * 10
            : numericValue.Value;
    }

    private static int? ParseInteger(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
            ? parsedValue
            : null;
    }

    private static int? ParseModelYear(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = Regex.Match(value, @"(?<!\d)\d{4}(?!\d)");
        var parsedYear = match.Success
            ? int.Parse(match.Value, CultureInfo.InvariantCulture)
            : (int?)null;

        return parsedYear.HasValue && IsValidModelYear(parsedYear.Value)
            ? parsedYear.Value
            : null;
    }

    private static bool IsValidModelYear(int year) =>
        year >= 1900 && year <= DateTime.UtcNow.Year + 1;

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
