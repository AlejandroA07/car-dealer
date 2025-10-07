using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task LoadManufacturerData(WestcoastCarsContext context)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (context.Manufacturers.Any()) return;

            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "Data", "json", "manufacturer.json");
            var json = System.IO.File.ReadAllText(path);
            var manufacturers = JsonSerializer.Deserialize<List<Manufacturer>>(json, options);

            if (manufacturers is not null && manufacturers.Count > 0)
            {
                await context.Manufacturers.AddRangeAsync(manufacturers);
                await context.SaveChangesAsync();
            }
        }

        public static async Task LoadVehicleData(WestcoastCarsContext context)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (context.Vehicles.Any()) return;

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

        public static async Task LoadFuelTypeData(WestcoastCarsContext context)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (context.FuelTypes.Any()) return;

            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "Data", "json", "fuelTypes.json");
            var json = System.IO.File.ReadAllText(path);
            var fueltypes = JsonSerializer.Deserialize<List<FuelType>>(json, options);

            if (fueltypes is not null && fueltypes.Count > 0)
            {
                await context.FuelTypes.AddRangeAsync(fueltypes);
                await context.SaveChangesAsync();
            }
        }

        public static async Task LoadTransmissionsData(WestcoastCarsContext context)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (context.TransmissionTypes.Any()) return;

            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "Data", "json", "transmissionTypes.json");
            var json = System.IO.File.ReadAllText(path);
            var transmissions = JsonSerializer.Deserialize<List<TransmissionType>>(json, options);

            if (transmissions is not null && transmissions.Count > 0)
            {
                await context.TransmissionTypes.AddRangeAsync(transmissions);
                await context.SaveChangesAsync();
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
}