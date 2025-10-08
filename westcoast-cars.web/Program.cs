using Microsoft.AspNetCore.DataProtection;
using westcoast_cars.web.Services;
using westcoast_cars.web.Handlers; // Added
using Microsoft.AspNetCore.Authentication; // Added
using Microsoft.AspNetCore.Authentication.Cookies; // Added

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure the HttpClient to connect to the API.
// It reads "ApiBaseUrl" from configuration (provided by docker-compose.yml or appsettings.json).
builder.Services.AddHttpClient("ApiClient", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<AuthHandler>(); // Add AuthHandler to the HttpClient pipeline

        // Configure the HttpClient for the Auth API.
        builder.Services.AddHttpClient("AuthApiClient", client =>
        {
            var authApiBaseUrl = builder.Configuration["AuthApiBaseUrl"];
            client.BaseAddress = new Uri(authApiBaseUrl);
        });

        // Register AuthService and inject the named HttpClient
        builder.Services.AddScoped<AuthService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("AuthApiClient");
            var configuration = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<AuthService>>();
            return new AuthService(httpClient, configuration, logger);
        });

builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IFuelTypeService, FuelTypeService>();
builder.Services.AddScoped<ITransmissionTypeService, TransmissionTypeService>();

builder.Services.AddHttpContextAccessor(); // Register IHttpContextAccessor
builder.Services.AddTransient<AuthHandler>(); // Register AuthHandler

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

// Configure data protection to persist keys.
// The path /app/keys is used within the Docker container.
// For local development, you might need to ensure this path is valid or configure a different one.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
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
