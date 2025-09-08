using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure the HttpClient to connect to the API.
// It reads "ApiBaseUrl" from configuration (provided by docker-compose.yml or appsettings.json).
builder.Services.AddHttpClient("ApiClient", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
