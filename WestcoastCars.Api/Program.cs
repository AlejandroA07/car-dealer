using System.Reflection;
using MediatR;
using System.Text;
using Microsoft.AspNetCore.Identity;
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
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Npgsql;
using System.Threading.RateLimiting;



using WestcoastCars.Api.Swagger.Examples;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using WestcoastCars.Api.Observability;

// ... other usings ...

using WestcoastCars.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddSingleton<AppTelemetry>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(AppTelemetry.ActivitySourceName);
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(AppTelemetry.MeterName)
            .AddPrometheusExporter();
    });

// Configure Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));

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

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql");

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddMemoryCache();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
});

if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddJsonConsole();
}

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<VehicleDtoExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<LoginRequestExample>();

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

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
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
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        if (context.Database.IsRelational() && !context.Database.IsSqlite())
        {
            await context.Database.MigrateAsync();

            if (context.Database.GetDbConnection() is NpgsqlConnection npgsqlConnection)
            {
                if (npgsqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await npgsqlConnection.OpenAsync();
                }

                await npgsqlConnection.ReloadTypesAsync(CancellationToken.None);
            }
        }
        else if (context.Database.IsSqlite())
        {
            await context.Database.EnsureCreatedAsync();
        }

        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var adminOptions = builder.Configuration.GetSection(AdminOptions.SectionName).Get<AdminOptions>();
        var adminPassword = adminOptions?.Password;

        if (!string.IsNullOrEmpty(adminPassword))
        {
            await IdentitySeedData.SeedRolesAndUsers(userManager, roleManager, adminPassword, logger);
        }
        else
        {
            logger.LogWarning("AdminSettings:Password not found. Skipping auth user seeding.");
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex,
            "Main API cannot start because the PostgreSQL database is unavailable or migration/seeding failed. Database: {Database}. Connection: {ConnectionString}. Verify PostgreSQL is running and check ConnectionStrings:DefaultConnection.",
            "westcoast_cars",
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

app.UseHttpLogging();

app.UseRateLimiter();

app.UseExceptionHandler("/error");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");
app.MapPrometheusScrapingEndpoint();
app.MapControllers();

app.Run();

static string SanitizeConnectionString(string connectionString)
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
