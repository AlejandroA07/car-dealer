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
        var json = System.IO.File.ReadAllText(path);
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
        var json = System.IO.File.ReadAllText(path);
        var vehicleDtos = JsonSerializer.Deserialize<List<VehicleSeedDto>>(json, options);

        if (vehicleDtos is null || !vehicleDtos.Any()) return;

        var manufacturers = await context.Manufacturers.ToDictionaryAsync(m => m.Id);
        var fuelTypes = await context.FuelTypes.ToDictionaryAsync(f => f.Id);
        var transmissionTypes = await context.TransmissionTypes.ToDictionaryAsync(t => t.Id);

        var vehicles = new List<Vehicle>();
        foreach (var dto in vehicleDtos)
        {
            var vehicle = new Vehicle
            {
                Id = dto.Id,
                RegistrationNumber = dto.RegistrationNumber,
                Model = dto.Model,
                ModelYear = dto.ModelYear,
                Mileage = dto.Mileage,
                ImageUrl = dto.ImageUrl,
                Value = dto.Value,
                Description = dto.Description,
                IsSold = dto.IsSold,
                ManufacturerId = dto.ManufacturerId,
                FuelTypeId = dto.FuelTypeId,
                TransmissionTypeId = dto.TransmissionTypeId,
                Manufacturer = manufacturers[dto.ManufacturerId],
                FuelType = fuelTypes[dto.FuelTypeId],
                TransmissionType = transmissionTypes[dto.TransmissionTypeId]
            };
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
        var json = System.IO.File.ReadAllText(path);
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
        var json = System.IO.File.ReadAllText(path);
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

    public static async Task EnsurePostgreSqlIdentitySequencesAsync(WestcoastCarsContext context)
    {
        if (!string.Equals(context.Database.ProviderName, "Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.Ordinal))
        {
            return;
        }

        var seededTables = new[]
        {
            "Manufacturers",
            "FuelTypes",
            "TransmissionTypes",
            "Vehicles"
        };

        foreach (var tableName in seededTables)
        {
            var sql = $"""
                SELECT setval(
                    pg_get_serial_sequence('"{tableName}"', 'Id'),
                    GREATEST(COALESCE((SELECT MAX("Id") FROM "{tableName}"), 0), 1),
                    EXISTS (SELECT 1 FROM "{tableName}")
                );
                """;

            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private class VehicleSeedDto
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public int ManufacturerId { get; set; }
        public string Model { get; set; } = string.Empty;
        public string ModelYear { get; set; } = string.Empty;
        public int FuelTypeId { get; set; }
        public int TransmissionTypeId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public bool IsSold { get; set; }
        public int Value { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
