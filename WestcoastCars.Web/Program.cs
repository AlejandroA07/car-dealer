using Microsoft.AspNetCore.DataProtection;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WestcoastCars.Web.Configurations;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Options
builder.Services.Configure<ServiceOptions>(builder.Configuration.GetSection(ServiceOptions.SectionName));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var serviceOptions = builder.Configuration.GetSection(ServiceOptions.SectionName).Get<ServiceOptions>() ?? throw new InvalidOperationException("ServiceOptions section is missing or invalid");

// Configure the HttpClient to connect to the API.
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(serviceOptions.ApiUrl);
    client.Timeout = TimeSpan.FromSeconds(3);
})
.AddHttpMessageHandler<AuthHandler>();

builder.Services.AddHttpClient("LongRunningApiClient", client =>
{
    client.BaseAddress = new Uri(serviceOptions.ApiUrl);
    client.Timeout = TimeSpan.FromMinutes(2);
})
.AddHttpMessageHandler<AuthHandler>();

// Configure the HttpClient for auth endpoints hosted by the main API.
builder.Services.AddHttpClient<IAuthService, AuthService>("AuthApiClient", client =>
{
    client.BaseAddress = new Uri(serviceOptions.ApiUrl);
    client.Timeout = TimeSpan.FromSeconds(3);
});

builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IFuelTypeService, FuelTypeService>();
builder.Services.AddScoped<ITransmissionTypeService, TransmissionTypeService>();
builder.Services.AddScoped<IServiceBookingService, ServiceBookingService>();

builder.Services.AddMemoryCache();
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
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.HttpOnly = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var token = context.Properties.GetTokenValue("access_token");
                if (string.IsNullOrEmpty(token)) return;

                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token)) return;

                var jwt = handler.ReadJwtToken(token);
                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
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

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://kit.fontawesome.com https://code.jquery.com https://cdnjs.cloudflare.com; " +
        "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://ka-f.fontawesome.com; " +
        "font-src 'self' https://ka-f.fontawesome.com; " +
        "connect-src 'self' https://ka-f.fontawesome.com; " +
        "img-src 'self' data: https:;";
    await next();
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
