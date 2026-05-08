using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Api.IntegrationTests;

public class FuelTypesIntegrationTests : IntegrationTestBase
{
    public FuelTypesIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task FuelTypeCrudFlow_ShouldCreateGetUpdateAndDeleteFuelType()
    {
        var client = await CreateAuthenticatedClientAsync();
        var fuelTypeName = $"Fuel-{Guid.NewGuid():N}"[..13];

        var createResponse = await client.PostAsJsonAsync("/api/v1/fueltypes", new NamedObjectDto { Name = fuelTypeName });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdFuelType = await createResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        createdFuelType.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/v1/fueltypes/{createdFuelType!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedFuelType = await getResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        fetchedFuelType.Should().NotBeNull();
        fetchedFuelType!.Name.Should().Be(fuelTypeName);

        var updatedName = $"{fuelTypeName}-Updated";
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/fueltypes/{createdFuelType.Id}", new NamedObjectDto
        {
            Id = createdFuelType.Id,
            Name = updatedName
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getUpdatedResponse = await _client.GetAsync($"/api/v1/fueltypes/{createdFuelType.Id}");
        getUpdatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedFuelType = await getUpdatedResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        updatedFuelType.Should().NotBeNull();
        updatedFuelType!.Name.Should().Be(updatedName);

        var deleteResponse = await client.DeleteAsync($"/api/v1/fueltypes/{createdFuelType.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync("/api/v1/fueltypes");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fuelTypes = await listResponse.Content.ReadFromJsonAsync<IEnumerable<NamedObjectDto>>();
        fuelTypes.Should().NotBeNull();
        fuelTypes!.Should().NotContain(fuelType => fuelType.Id == createdFuelType.Id);
    }

    [Fact]
    public async Task AddFuelType_ShouldReturnConflict_WhenNameDiffersOnlyByCase()
    {
        var client = await CreateAuthenticatedClientAsync();
        var fuelTypeName = $"Fuel-{Guid.NewGuid():N}"[..13].ToUpperInvariant();

        var firstResponse = await client.PostAsJsonAsync("/api/v1/fueltypes", new NamedObjectDto { Name = fuelTypeName });
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/fueltypes", new NamedObjectDto { Name = fuelTypeName.ToLowerInvariant() });
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
