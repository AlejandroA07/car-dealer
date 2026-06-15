using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WestcoastCars.Api.IntegrationTests;

public class HealthIntegrationTests(CustomWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy_WhenDatabaseIsAvailable()
    {
        var response = await _client.GetAsync("/health/ready");

        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthChecks_ShouldIncludePostgreSqlDependency()
    {
        using var scope = _factory.Services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();

        var report = await healthCheckService.CheckHealthAsync();

        report.Status.Should().Be(HealthStatus.Healthy);
        report.Entries.Keys.Should().Contain("postgresql");
    }
}
