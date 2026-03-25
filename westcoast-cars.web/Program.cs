using Microsoft.AspNetCore.DataProtection;
using westcoast_cars.web.Services;
using westcoast_cars.web.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WestcoastCars.Web.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Options
builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection(ServiceOptions.SectionName));
var serviceOptions = builder.Configuration.GetSection(ServiceOptions.SectionName).Get<ServiceOptions>() ?? throw new InvalidOperationException("ServiceOptions section is missing or invalid");

// Configure the HttpClient to connect to the API.
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(serviceOptions.ApiUrl);
})
.AddHttpMessageHandler<AuthHandler>();

// Configure the HttpClient for the Auth API.
builder.Services.AddHttpClient<IAuthService, AuthService>("AuthApiClient", client =>
{
    client.BaseAddress = new Uri(serviceOptions.AuthUrl);
});

builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IFuelTypeService, FuelTypeService>();
builder.Services.AddScoped<ITransmissionTypeService, TransmissionTypeService>();
builder.Services.AddScoped<IServiceBookingService, ServiceBookingService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHandler>();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/accessdenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// Configure data protection to persist keys from configuration.
var keysPath = builder.Configuration["DataProtectionPath"] ?? "dpkeys";
if (!Path.IsPathRooted(keysPath))
{
    keysPath = Path.Combine(Directory.GetCurrentDirectory(), keysPath);
}

if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("WestcoastCars");

var app = builder.Build();

// Database and seeding logic has been removed.
// The API is now responsible for its own data.

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
