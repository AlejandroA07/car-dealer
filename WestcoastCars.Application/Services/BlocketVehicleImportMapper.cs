using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;

namespace WestcoastCars.Application.Services;

public class BlocketVehicleImportMapper : IBlocketVehicleImportMapper
{
    private const string DefaultImageUrl = "/images/no-car.png";
    private readonly ILogger<BlocketVehicleImportMapper> _logger;

    public BlocketVehicleImportMapper(ILogger<BlocketVehicleImportMapper> logger)
    {
        _logger = logger;
    }

    // Blocket uses these Swedish placeholders when data is missing — treat all as unknown
    private static readonly HashSet<string> BlocketUnknownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "okänd", "unknown", "ej angiven", "saknas", "övrigt"
    };

    internal static readonly Dictionary<string, string> FuelMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Bensin"] = "Bensin",
        ["Diesel"] = "Diesel",
        ["El"] = "El",
        ["Elektrisk"] = "El",
        ["Bensin/El"] = "Bensin/El",
        ["Diesel/El"] = "Diesel/El",
        ["Hybrid"] = "Hybrid",
        ["Laddhybrid"] = "Laddhybrid",
        ["Etanol"] = "Etanol",
        ["Gas"] = "Gas",
        ["Biogas"] = "Biogas",
        ["Vätgas"] = "Vätgas"
    };

    internal static readonly Dictionary<string, string> TransmissionMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Automatisk"] = "Automat",
        ["Automat"] = "Automat",
        ["Manuell"] = "Manuell",
        ["Manuel"] = "Manuell"
    };

    public BlocketVehicleImportData Map(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails, DateTime importedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(searchItem);

        if (adDetails?.Specifications is { Count: > 0 })
            _logger.LogDebug("Blocket specs for {Id}: {Keys}",
                searchItem.Id,
                string.Join(" | ", adDetails.Specifications.Select(kv => $"{kv.Key}={kv.Value}")));

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
            GalleryUrls = NormalizeGalleryUrls(searchItem, adDetails),
            ExternalListingId = searchItem.Id,
            Source = "Blocket",
            SourceUrl = NormalizeOptional(searchItem.CanonicalUrl)
                ?? NormalizeOptional(adDetails?.Url),
            PublishedAt = NormalizePublishedAt(searchItem.Timestamp),
            ImportedAt = importedAtUtc,
            Color = NormalizeColor(adDetails),
            WheelDrive = GetSpecification(adDetails, "Drivhjul")
                ?? GetSpecification(adDetails, "Drivning"),
            Horsepower = ParseHorsepower(GetSpecification(adDetails, "Effekt")),
            BodyType = GetSpecification(adDetails, "Biltyp")
                ?? GetSpecification(adDetails, "Chassinummer"),
            Doors = ParseInteger(GetSpecification(adDetails, "Antal dörrar")),
            EngineVolume = GetSpecification(adDetails, "Motorvolym"),
            City = NormalizeOptional(searchItem.Location),
            Equipment = adDetails?.Equipment ?? [],
            Seats = ParseInteger(GetSpecification(adDetails, "Säten")),
            MaxTrailerWeight = ParseInteger(GetSpecification(adDetails, "Max trailervikt")),
            OwnerCount = ParseInteger(GetSpecification(adDetails, "Antal ägare")),
            LastInspectionDate = ParseDate(GetSpecification(adDetails, "Senaste besiktningsdatum")),
            NextInspectionDate = ParseDate(GetSpecification(adDetails, "Nästa besiktningsdatum"))
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
            return string.Equals(searchItem.MileageUnit, "km", StringComparison.OrdinalIgnoreCase)
                ? searchItem.Mileage.Value
                : searchItem.Mileage.Value * 10;
        }

        return ParseMileage(GetSpecification(adDetails, "Miltal"))
            ?? ParseMileage(adDetails?.Mileage)
            ?? 0;
    }

    private static string NormalizeImageUrl(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails)
    {
        var imageUrl = searchItem.Image?.Url
            ?? adDetails?.Image?.Url
            ?? adDetails?.Images.FirstOrDefault()?.Url;

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
        return NormalizeOptional(adDetails?.Description)
            ?? NormalizeOptional(searchItem.ModelSpecification)
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
        return GetSpecification(adDetails, "Färg");
    }

    // Blocket "Effekt" is typically "150 hk" or "110 kW / 150 hk" — extract the hk value.
    // Falls back to the first integer found if no explicit hk suffix is present.
    private static int? ParseHorsepower(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var hkMatch = System.Text.RegularExpressions.Regex.Match(value, @"(\d+)\s*hk", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (hkMatch.Success && int.TryParse(hkMatch.Groups[1].Value, out var hk))
            return hk;

        return ParseInteger(value);
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

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateOnly.TryParseExact(value.Trim(),
            ["yyyy-MM-dd", "yyyy-MM", "dd/MM/yyyy", "dd-MM-yyyy"],
            CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d)
            ? d : null;
    }

    private static bool IsValidModelYear(int year) =>
        year >= 1900 && year <= DateTime.UtcNow.Year + 1;

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<string> NormalizeGalleryUrls(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails)
    {
        var urls = adDetails?.Images
            .Select(img => img.Url)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u!)
            .Distinct()
            .ToList() ?? [];

        // If the ad endpoint returned no gallery, fall back to the search thumbnail
        if (urls.Count == 0 && !string.IsNullOrWhiteSpace(searchItem.Image?.Url))
            urls.Add(searchItem.Image.Url);

        return urls;
    }
}
