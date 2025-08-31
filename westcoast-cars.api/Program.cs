using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add database support
builder.Services.AddDbContext<WestcoastCarsContext>(options => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Register Unit of Work for dependency injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<WestcoastCarsContext>();
    try
    {
        try
        {
            await context.Database.MigrateAsync();
            await SeedData.LoadManufacturerData(context);
            await SeedData.LoadFuelTypeData(context);
            await SeedData.LoadTransmissionsData(context);
            await SeedData.LoadVehicleData(context);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
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
