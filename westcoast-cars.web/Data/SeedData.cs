using System.Text.Json;
using westcoast_cars.web.Models;

namespace westcoast_cars.web.Data
{
    public static class SeedData
    {
        public static async Task LoadManufacturerData(WestcoastCarsContext context) 
        {
            // 1. Case sensitive
            var options = new JsonSerializerOptions{
                PropertyNameCaseInsensitive = true
            };

            // 2. load data if db is empty
            

            // 3. read json info from vehicles.json file
            var json = System.IO.File.ReadAllText("Data/json/manufacturer.json");

            // 4. convert json object to a list of vehicleModel object
            var manufacturers = JsonSerializer.Deserialize<List<ManufacturerModel>>(json, options);

            // 5. Send vehicleModel list to db
            if(manufacturers is not null && manufacturers.Count > 0)
            {
                // AddRangeAsync is a method that adds the args to a memory where ef can follow an sincronyze with (Chain's tracking)
                await context.Manufacturers.AddRangeAsync(manufacturers);
                await context.SaveChangesAsync();
            }
        }
        
        public static async Task LoadVehicleData(WestcoastCarsContext context) 
        {
            // 1. Case sensitive
            var options = new JsonSerializerOptions{
                PropertyNameCaseInsensitive = true
            };

            // 2. load data if db is empty
            

            // 3. read json info from vehicles.json file
            var json = System.IO.File.ReadAllText("Data/json/vehicles.json");

            // 4. convert json object to a list of vehicleModel object
            var vehicels = JsonSerializer.Deserialize<List<VehicleModel>>(json, options);

            // 5. Send vehicleModel list to db
            if(vehicels is not null && vehicels.Count > 0)
            {
                // AddRangeAsync is a method that adds the args to a memory where ef can follow an sincronyze with (Chain's tracking)
                await context.Vehicles.AddRangeAsync(vehicels);
                await context.SaveChangesAsync();
            }
        }

        public static async Task LoadFuelTypeData(WestcoastCarsContext context)
        {
            // Steg 1.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Steg 2. Vill endast ladda data om vår vehicles tabell är tom...
            

            // Steg 3. Läsa in json informationen ifrån vår vehicles.json fil...
            var json = System.IO.File.ReadAllText("Data/json/fuelTypes.json");

            // Steg 4. Omvandla json objekten till en lista av VehicleModel objekt...
            var fueltypes = JsonSerializer.Deserialize<List<FuelTypeModel>>(json, options);

            // Steg 5. Skicka listan med VehicleModel objekt till databasen...
            if (fueltypes is not null && fueltypes.Count > 0)
            {
                await context.FuelTypes.AddRangeAsync(fueltypes);
                await context.SaveChangesAsync();
            }
        }

        public static async Task LoadTransmissionsData(WestcoastCarsContext context)
        {
            // Steg 1.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Steg 2. Vill endast ladda data om vår vehicles tabell är tom...
            

            // Steg 3. Läsa in json informationen ifrån vår vehicles.json fil...
            var json = System.IO.File.ReadAllText("Data/json/transmissionTypes.json");

            // Steg 4. Omvandla json objekten till en lista av VehicleModel objekt...
            var transmissions = JsonSerializer.Deserialize<List<TransmissionsTypeModel>>(json, options);

            // Steg 5. Skicka listan med VehicleModel objekt till databasen...
            if (transmissions is not null && transmissions.Count > 0)
            {
                await context.TransmissionsTypes.AddRangeAsync(transmissions);
                await context.SaveChangesAsync();
            }
        }
    }
}

/* 
    dotnet ef database drop
    delete migration folder
    dotnet ef migrations add InicialCreate -o Data/Migrations
    dotnet ef database update
    dotnet ef migrations remove
 */