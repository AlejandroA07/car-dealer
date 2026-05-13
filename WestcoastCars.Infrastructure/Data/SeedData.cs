using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Infrastructure.Data;

public static class SeedData
{
    public static async Task<bool> HasVehiclesAsync(WestcoastCarsContext context)
    {
        if (!context.Database.IsRelational())
            return await context.Vehicles.AnyAsync();

        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT EXISTS (SELECT 1 FROM \"Vehicles\");";
            return Convert.ToBoolean(await command.ExecuteScalarAsync());
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    public static async Task LoadVehicleData(WestcoastCarsContext context)
    {
        if (await context.Vehicles.AnyAsync()) return;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "json", "vehicles.json");
        var json = await File.ReadAllTextAsync(path);
        var vehicleDtos = JsonSerializer.Deserialize<List<VehicleSeedDto>>(json, options);
        if (vehicleDtos is null || vehicleDtos.Count == 0) return;

        // Upsert manufacturers derived from vehicle data
        var existingManufacturers = (await context.Manufacturers.ToListAsync())
            .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in vehicleDtos.Select(v => v.Manufacturer).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (existingManufacturers.ContainsKey(name)) continue;
            var entity = new Manufacturer { Name = name };
            context.Manufacturers.Add(entity);
            existingManufacturers[name] = entity;
        }
        await context.SaveChangesAsync();

        // Upsert fuel types derived from vehicle data
        var existingFuelTypes = (await context.FuelTypes.ToListAsync())
            .ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in vehicleDtos.Select(v => v.FuelType).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (existingFuelTypes.ContainsKey(name)) continue;
            var entity = new FuelType { Name = name };
            context.FuelTypes.Add(entity);
            existingFuelTypes[name] = entity;
        }
        await context.SaveChangesAsync();

        // Upsert transmission types derived from vehicle data
        var existingTransmissions = (await context.TransmissionTypes.ToListAsync())
            .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in vehicleDtos.Select(v => v.TransmissionType).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (existingTransmissions.ContainsKey(name)) continue;
            var entity = new TransmissionType { Name = name };
            context.TransmissionTypes.Add(entity);
            existingTransmissions[name] = entity;
        }
        await context.SaveChangesAsync();

        // Create vehicles using resolved lookups (EF populates IDs after each SaveChanges above)
        var vehicles = new List<Vehicle>();
        foreach (var dto in vehicleDtos)
        {
            var manufacturer    = ResolveSeedLookup(existingManufacturers,  dto.Manufacturer,     dto.RegistrationNumber, "manufacturer");
            var fuelType        = ResolveSeedLookup(existingFuelTypes,       dto.FuelType,         dto.RegistrationNumber, "fuel type");
            var transmissionType = ResolveSeedLookup(existingTransmissions,  dto.TransmissionType, dto.RegistrationNumber, "transmission type");

            var vehicle = new Vehicle
            {
                RegistrationNumber   = dto.RegistrationNumber,
                Model                = dto.Model,
                ModelYear            = dto.ModelYear,
                Mileage              = dto.Mileage,
                ImageUrl             = dto.ImageUrl,
                Price                = dto.Price,
                Description          = dto.Description,
                ManufacturerId       = manufacturer.Id,
                FuelTypeId           = fuelType.Id,
                TransmissionTypeId   = transmissionType.Id,
                Manufacturer         = manufacturer,
                FuelType             = fuelType,
                TransmissionType     = transmissionType
            };
            if (dto.IsSold) vehicle.MarkAsSold();
            vehicles.Add(vehicle);
        }

        await context.Vehicles.AddRangeAsync(vehicles);
        await context.SaveChangesAsync();
    }

    private static TLookup ResolveSeedLookup<TLookup>(
        IDictionary<string, TLookup> lookupsByName,
        string lookupName,
        string vehicleRegistrationNumber,
        string lookupType)
        where TLookup : class
    {
        if (lookupsByName.TryGetValue(lookupName, out var lookup))
            return lookup;

        throw new InvalidOperationException(
            $"Vehicle seed '{vehicleRegistrationNumber}' references unknown {lookupType} '{lookupName}'.");
    }

    private class VehicleSeedDto
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Manufacturer       { get; set; } = string.Empty;
        public string Model              { get; set; } = string.Empty;
        public int    ModelYear          { get; set; }
        public string FuelType           { get; set; } = string.Empty;
        public string TransmissionType   { get; set; } = string.Empty;
        public string ImageUrl           { get; set; } = string.Empty;
        public int    Mileage            { get; set; }
        public bool   IsSold             { get; set; }
        public int    Price              { get; set; }
        public string Description        { get; set; } = string.Empty;
    }
}
