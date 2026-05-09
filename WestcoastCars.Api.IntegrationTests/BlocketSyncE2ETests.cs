using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WestcoastCars.Application.Features.Vehicles.Commands.RefreshInventoryFromBlocket;
using WestcoastCars.Infrastructure.Data;
using Xunit;

namespace WestcoastCars.Api.IntegrationTests;

[Trait("Category", "ExternalE2E")]
public class BlocketSyncE2ETests : IntegrationTestBase
{
    public BlocketSyncE2ETests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [OptionalBlocketE2EFact]
    public async Task SyncBlocket_ShouldReplaceVehicles_AndCapTo50()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/vehicles/import/blocket", new RefreshInventoryFromBlocketCommand
        {
            Limit = 50,
            Models = "VOLVO",
            Locations = "STOCKHOLM"
        });

        var responseBody = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        var result = await response.Content.ReadFromJsonAsync<RefreshInventoryFromBlocketResult>();
        result.Should().NotBeNull();
        result!.AppliedLimit.Should().Be(50);
        result.TotalImported.Should().Be(50);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();
            var totalVehicles = context.Vehicles.Count();
            totalVehicles.Should().Be(50);
            context.Vehicles.All(vehicle => vehicle.Source == "Blocket").Should().BeTrue();
        }

        // Run again and verify cap still holds
        var secondResponse = await client.PostAsJsonAsync("/api/v1/vehicles/import/blocket", new RefreshInventoryFromBlocketCommand
        {
            Limit = 50,
            Models = "VOLVO",
            Locations = "STOCKHOLM"
        });

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();
            context.Vehicles.Count().Should().Be(50);
        }
    }
}
