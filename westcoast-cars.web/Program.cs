using Microsoft.EntityFrameworkCore;
using westcoast_cars.web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient();

// set up db configuration/DbConnectionStringPipeline
builder.Services.AddDbContext<WestcoastCarsContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// load data to db, this goes after var app = builder.Build();

// this is for access to load SeedData Context
// Here i create an execution environment that lives as long as I need it (variable)
using var scope = app.Services.CreateScope();
// The scope above gives me access to a service provider which i'll use for getting WestcoastCarsContex
var services = scope.ServiceProvider;

try
{
    // here is the type
    var context = services.GetRequiredService<WestcoastCarsContext>();
    // here i do a migration to the db
    await context.Database.MigrateAsync();
    // Here runs LoadData with the context
    await SeedData.LoadManufacturerData(context);
    await SeedData.LoadFuelTypeData(context);
    await SeedData.LoadTransmissionsData(context);
    await SeedData.LoadVehicleData(context);
    
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
