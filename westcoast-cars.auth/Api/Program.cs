using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WestcoastCars.Auth.Infrastructure;
using WestcoastCars.Auth.Infrastructure.Authentication;
using WestcoastCars.Auth.Infrastructure.Data;
using WestcoastCars.Auth.Api.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using WestcoastCars.Auth.Api.Swagger.Examples;
using Swashbuckle.AspNetCore.Filters;

// ... other usings ...

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// Configure Options
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

builder.Services.AddControllers();

builder.Services.AddSwaggerExamplesFromAssemblyOf<LoginRequestExample>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Westcoast Cars Auth API", 
        Version = "v1",
        Description = "Authentication and Authorization service for Westcoast Cars."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    var contractsXmlPath = Path.Combine(AppContext.BaseDirectory, "WestcoastCars.Auth.Contracts.xml");
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

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() 
    ?? throw new InvalidOperationException("JwtSettings section is missing or invalid");

builder.Services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret))
    });

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = serviceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.MigrateAsync();

        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        var adminOptions = builder.Configuration.GetSection(AdminOptions.SectionName).Get<AdminOptions>();
        var adminPassword = adminOptions?.Password; 
        
        if (!string.IsNullOrEmpty(adminPassword))
        {
            await SeedData.SeedRolesAndAdminUser(dbContext, userManager, roleManager, adminPassword, logger);
        }
        else
        {
             logger.LogWarning("AdminSettings:Password not found. Skipping user seeding.");
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex,
            "Auth API cannot start because the auth database is unavailable or migration/seeding failed. Database: {Database}. Connection: {ConnectionString}. Start MySQL, verify the 'westcoast_auth' database exists, and check ConnectionStrings:DefaultConnection.",
            "westcoast_auth",
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Westcoast Cars Auth API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Westcoast Cars Auth API Documentation";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });
}

app.UseHttpsRedirection();

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
