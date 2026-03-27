using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WestcoastCars.Infrastructure.Data;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace WestcoastCars.Api.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = "Data Source=TestDb;Mode=Memory;Cache=Shared";
        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        builder.UseSetting("JwtSettings:Secret", "super-secret-key-for-testing-purposes-only-123");
        builder.UseSetting("JwtSettings:Issuer", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:Audience", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "60");

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<WestcoastCarsContext>();
                db.Database.EnsureCreated();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
