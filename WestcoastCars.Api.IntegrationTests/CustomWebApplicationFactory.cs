using DotNet.Testcontainers.Builders;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace WestcoastCars.Api.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    private readonly string _sqliteFallbackConnectionString = $"Data Source=WestcoastCarsIntegrationTests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    private PostgreSqlContainer? _dbContainer;
    private SqliteConnection? _sqliteFallbackConnection;
    private string _connectionString = string.Empty;

    public bool UsesSqliteFallback { get; private set; }

    async Task IAsyncLifetime.InitializeAsync()
    {
        try
        {
            _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
                .Build();

            await _dbContainer.StartAsync();
            _connectionString = _dbContainer.GetConnectionString();
        }
        catch (DockerUnavailableException)
        {
            await EnableSqliteFallbackAsync();
        }
        catch (AggregateException ex) when (ex.InnerExceptions.OfType<DockerUnavailableException>().Any())
        {
            await EnableSqliteFallbackAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.UseSetting("JwtSettings:Secret", "super-secret-key-for-testing-purposes-only-123");
        builder.UseSetting("JwtSettings:Issuer", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:Audience", "WestcoastCars.Auth");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "60");
        builder.UseSetting("AdminSettings:Password", "Password123!");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_dbContainer is not null)
        {
            await _dbContainer.DisposeAsync();
        }

        if (_sqliteFallbackConnection is not null)
        {
            await _sqliteFallbackConnection.DisposeAsync();
        }
    }

    private async Task EnableSqliteFallbackAsync()
    {
        UsesSqliteFallback = true;
        _connectionString = _sqliteFallbackConnectionString;
        _sqliteFallbackConnection = new SqliteConnection(_sqliteFallbackConnectionString);
        await _sqliteFallbackConnection.OpenAsync();
    }
}
