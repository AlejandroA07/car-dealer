using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Application.Services;

namespace WestcoastCars.Application.Helpers;

internal static class BlocketFilterHelpers
{
    internal static bool PassesMileageFilter(BlocketCarSearchItem item, int? min, int? max)
    {
        if (!min.HasValue && !max.HasValue) return true;
        if (!item.Mileage.HasValue) return false;

        var km = string.Equals(item.MileageUnit, "km", StringComparison.OrdinalIgnoreCase)
            ? item.Mileage.Value
            : item.Mileage.Value * 10;

        return (!min.HasValue || km >= min.Value) && (!max.HasValue || km < max.Value);
    }

    internal static bool PassesTransmissionFilter(string? raw, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return true;
        if (string.IsNullOrWhiteSpace(raw)) return true; // unknown ≠ wrong — seller didn't fill in the field
        var normalized = BlocketVehicleImportMapper.TransmissionMappings.TryGetValue(raw.Trim(), out var mapped)
            ? mapped : raw.Trim();
        var normalizedFilter = BlocketVehicleImportMapper.TransmissionMappings.TryGetValue(filter.Trim(), out var mappedFilter)
            ? mappedFilter : filter.Trim();
        return normalized.Equals(normalizedFilter, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool PassesFuelFilter(string? raw, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return true;
        if (string.IsNullOrWhiteSpace(raw)) return true; // unknown ≠ wrong
        var normalized = BlocketVehicleImportMapper.FuelMappings.TryGetValue(raw.Trim(), out var mapped)
            ? mapped : raw.Trim();
        var normalizedFilter = BlocketVehicleImportMapper.FuelMappings.TryGetValue(filter.Trim(), out var mappedFilter)
            ? mappedFilter : filter.Trim();
        return normalized.Equals(normalizedFilter, StringComparison.OrdinalIgnoreCase);
    }

    internal static int NormalizeMileageKm(int mileage, string? unit) =>
        string.Equals(unit, "km", StringComparison.OrdinalIgnoreCase)
            ? mileage
            : mileage * 10;
}
