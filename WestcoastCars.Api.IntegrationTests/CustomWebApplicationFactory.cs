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
    private SqliteConnection? _businessConnection;
    private SqliteConnection? _authConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var businessConnectionString = $"Data Source=TestDb_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var authConnectionString = $"Data Source=AuthTestDb_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        _businessConnection = new SqliteConnection(businessConnectionString);
        _authConnection = new SqliteConnection(authConnectionString);
        _businessConnection.Open();
        _authConnection.Open();

        builder.UseSetting("ConnectionStrings:DefaultConnection", businessConnectionString);
        builder.UseSetting("ConnectionStrings:AuthConnection", authConnectionString);
        builder.UseSetting("JwtSettings:Secret", "super-secret-key-for-testing-purposes-only-123");
        builder.UseSetting("JwtSettings:Issuer", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:Audience", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "60");
        builder.UseSetting("AdminSettings:Password", "Password123!");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _businessConnection?.Close();
            _businessConnection?.Dispose();
            _authConnection?.Close();
            _authConnection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
