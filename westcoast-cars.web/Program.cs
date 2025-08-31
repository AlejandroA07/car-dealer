// Las sentencias 'using' para EntityFrameworkCore y la carpeta Data han sido eliminadas.

var builder = WebApplication.CreateBuilder(args);

// Añadir servicios al contenedor.

// Configura el HttpClient para conectarse a la API.
// Lee "ApiBaseUrl" desde la configuración (provista por docker-compose.yml o appsettings.json).
builder.Services.AddHttpClient("ApiClient", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];

    // Un valor de respaldo por si se ejecuta localmente fuera de Docker.
    if (string.IsNullOrEmpty(baseUrl))
    {
        // Esta URL debe coincidir con la de tu API al ejecutarla localmente.
        baseUrl = "http://localhost:5001"; 
    }
    
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Toda la lógica de base de datos y 'seeding' ha sido eliminada.
// La API es ahora responsable de sus propios datos.

// Configura el pipeline de peticiones HTTP.
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
