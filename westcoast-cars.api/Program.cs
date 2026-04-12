using System.Reflection;
using MediatR;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WestcoastCars.Api.Configurations;

using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Data;
using WestcoastCars.Infrastructure.Repositories;
using WestcoastCars.Infrastructure;
using FluentValidation;
using WestcoastCars.Application.Common.Behaviors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi.Models;



using WestcoastCars.Api.Swagger.Examples;
using Swashbuckle.AspNetCore.Filters;

// ... other usings ...

using WestcoastCars.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Configure Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

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

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<VehicleDtoExample>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Westcoast Cars API", 
        Version = "v1",
        Description = "API for managing vehicle inventory, manufacturers, and service bookings."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    var contractsXmlPath = Path.Combine(AppContext.BaseDirectory, "WestcoastCars.Contracts.xml");
    if (File.Exists(contractsXmlPath))
        c.IncludeXmlComments(contractsXmlPath);

    c.ExampleFilters();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: {token}. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? throw new InvalidOperationException("JwtOptions section is missing or invalid");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
    };
});


var app = builder.Build();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<WestcoastCarsContext>();
    try
    {
        if (context.Database.IsRelational() && !context.Database.IsSqlite())
        {
            await context.Database.MigrateAsync();
        }
        else if (context.Database.IsSqlite())
        {
            await context.Database.EnsureCreatedAsync();
        }
        
        await SeedData.LoadManufacturerData(context);
        await SeedData.LoadFuelTypeData(context);
        await SeedData.LoadTransmissionsData(context);
        await SeedData.LoadVehicleData(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex,
            "Main API cannot start because the vehicle database is unavailable or migration/seeding failed. Database: {Database}. Connection: {ConnectionString}. Start MySQL, verify the 'westcoast_cars_db' database exists, and check ConnectionStrings:DefaultConnection.",
            "westcoast_cars_db",
            SanitizeConnectionString(builder.Configuration.GetConnectionString("DefaultConnection")));
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Westcoast Cars API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Westcoast Cars API Documentation";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });
}

app.UseStaticFiles();

app.UseExceptionHandler("/error");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string SanitizeConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return "(missing)";
    }

    try
    {
        var builder = new System.Data.Common.DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        foreach (var key in new[] { "Password", "Pwd" })
        {
            if (builder.ContainsKey(key))
            {
                builder[key] = "***";
            }
        }

        return builder.ConnectionString;
    }
    catch
    {
        return "(configured but could not be sanitized)";
    }
}

public partial class Program { }
