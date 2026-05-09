using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Infrastructure.Data;

public static class SeedData
{
    public record SeedPresence(
        bool HasManufacturers,
        bool HasFuelTypes,
        bool HasTransmissionTypes,
        bool HasVehicles);

    public static async Task<SeedPresence> GetSeedPresenceAsync(WestcoastCarsContext context)
    {
        if (!context.Database.IsRelational())
        {
            return new SeedPresence(
                await context.Manufacturers.AnyAsync(),
                await context.FuelTypes.AnyAsync(),
                await context.TransmissionTypes.AnyAsync(),
                await context.Vehicles.AnyAsync());
        }

        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT
                    EXISTS (SELECT 1 FROM "Manufacturers"),
                    EXISTS (SELECT 1 FROM "FuelTypes"),
                    EXISTS (SELECT 1 FROM "TransmissionTypes"),
                    EXISTS (SELECT 1 FROM "Vehicles");
                """;

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return new SeedPresence(false, false, false, false);
            }

            return new SeedPresence(
                Convert.ToBoolean(reader.GetValue(0)),
                Convert.ToBoolean(reader.GetValue(1)),
                Convert.ToBoolean(reader.GetValue(2)),
                Convert.ToBoolean(reader.GetValue(3)));
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    public static async Task LoadManufacturerData(WestcoastCarsContext context, bool hasManufacturers)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (hasManufacturers) return;

        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Data", "json", "manufacturer.json");
        var json = await System.IO.File.ReadAllTextAsync(path);
        var manufacturers = JsonSerializer.Deserialize<List<Manufacturer>>(json, options);

        if (manufacturers is not null && manufacturers.Count > 0)
        {
            var distinctManufacturers = manufacturers
                .GroupBy(manufacturer => manufacturer.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            await context.Manufacturers.AddRangeAsync(distinctManufacturers);
            await context.SaveChangesAsync();
        }
    }

    public static async Task LoadVehicleData(WestcoastCarsContext context, bool hasVehicles)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (hasVehicles) return;

        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Data", "json", "vehicles.json");
        var json = await System.IO.File.ReadAllTextAsync(path);
        var vehicleDtos = JsonSerializer.Deserialize<List<VehicleSeedDto>>(json, options);

        if (vehicleDtos is null || !vehicleDtos.Any()) return;

        var manufacturers = (await context.Manufacturers.ToListAsync())
            .ToDictionary(manufacturer => manufacturer.Name, StringComparer.OrdinalIgnoreCase);
        var fuelTypes = (await context.FuelTypes.ToListAsync())
            .ToDictionary(fuelType => fuelType.Name, StringComparer.OrdinalIgnoreCase);
        var transmissionTypes = (await context.TransmissionTypes.ToListAsync())
            .ToDictionary(transmissionType => transmissionType.Name, StringComparer.OrdinalIgnoreCase);

        var vehicles = new List<Vehicle>();
        foreach (var dto in vehicleDtos)
        {
            var manufacturer = ResolveSeedLookup(manufacturers, dto.Manufacturer, dto.RegistrationNumber, "manufacturer");
            var fuelType = ResolveSeedLookup(fuelTypes, dto.FuelType, dto.RegistrationNumber, "fuel type");
            var transmissionType = ResolveSeedLookup(transmissionTypes, dto.TransmissionType, dto.RegistrationNumber, "transmission type");

            var vehicle = new Vehicle
            {
                RegistrationNumber = dto.RegistrationNumber,
                Model = dto.Model,
                ModelYear = dto.ModelYear,
                Mileage = dto.Mileage,
                ImageUrl = dto.ImageUrl,
                Price = dto.Price,
                Description = dto.Description,
                ManufacturerId = manufacturer.Id,
                FuelTypeId = fuelType.Id,
                TransmissionTypeId = transmissionType.Id,
                Manufacturer = manufacturer,
                FuelType = fuelType,
                TransmissionType = transmissionType
            };
            if (dto.IsSold) vehicle.MarkAsSold();
            vehicles.Add(vehicle);
        }

        await context.Vehicles.AddRangeAsync(vehicles);
        await context.SaveChangesAsync();
    }

    public static async Task LoadFuelTypeData(WestcoastCarsContext context, bool hasFuelTypes)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (hasFuelTypes) return;

        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Data", "json", "fuelTypes.json");
        var json = await System.IO.File.ReadAllTextAsync(path);
        var fueltypes = JsonSerializer.Deserialize<List<FuelType>>(json, options);

        if (fueltypes is not null && fueltypes.Count > 0)
        {
            var distinctFuelTypes = fueltypes
                .GroupBy(fuelType => fuelType.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            await context.FuelTypes.AddRangeAsync(distinctFuelTypes);
            await context.SaveChangesAsync();
        }
    }

    public static async Task LoadTransmissionsData(WestcoastCarsContext context, bool hasTransmissionTypes)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (hasTransmissionTypes) return;

        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Data", "json", "transmissionTypes.json");
        var json = await System.IO.File.ReadAllTextAsync(path);
        var transmissions = JsonSerializer.Deserialize<List<TransmissionType>>(json, options);

        if (transmissions is not null && transmissions.Count > 0)
        {
            var distinctTransmissions = transmissions
                .GroupBy(transmission => transmission.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            await context.TransmissionTypes.AddRangeAsync(distinctTransmissions);
            await context.SaveChangesAsync();
        }
    }

    private static TLookup ResolveSeedLookup<TLookup>(
        IDictionary<string, TLookup> lookupsByName,
        string lookupName,
        string vehicleRegistrationNumber,
        string lookupType)
        where TLookup : class
    {
        if (lookupsByName.TryGetValue(lookupName, out var lookup))
        {
            return lookup;
        }

        throw new InvalidOperationException(
            $"Vehicle seed '{vehicleRegistrationNumber}' references unknown {lookupType} '{lookupName}'.");
    }

    private class VehicleSeedDto
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int ModelYear { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public string TransmissionType { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public bool IsSold { get; set; }
        public int Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
