using System.Text.Json;

namespace WestcoastCars.Application.Common.Helpers;

internal static class VehicleFieldSerializer
{
    internal static string? ToJsonArray(string? newlineSeparated)
    {
        if (string.IsNullOrWhiteSpace(newlineSeparated)) return null;
        var items = newlineSeparated
            .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
        return items.Count > 0 ? JsonSerializer.Serialize(items) : null;
    }

    internal static string? ToJsonArray(IReadOnlyList<string> items) =>
        items.Count > 0 ? JsonSerializer.Serialize(items) : null;
}
