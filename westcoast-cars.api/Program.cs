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
