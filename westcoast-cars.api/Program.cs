using Microsoft.EntityFrameworkCore;
using westcoast_cars.api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add database support
builder.Services.AddDbContext<WestcoastCarsContext>(options => {
    var connectionString = builder.Configuration.GetConnectionString("MySql");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Load data into database
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<WestcoastCarsContext>();
    // Compares db context with the migration
    await context.Database.MigrateAsync();

    await SeedData.LoadTransmissionsData(context);
    await SeedData.LoadFuelTypeData(context);
    await SeedData.LoadManufacturerData(context);
    await SeedData.LoadVehicleData(context);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
