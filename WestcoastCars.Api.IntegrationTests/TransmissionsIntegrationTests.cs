using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Api.IntegrationTests;

public class TransmissionsIntegrationTests : IntegrationTestBase
{
    public TransmissionsIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task TransmissionCrudFlow_ShouldCreateGetUpdateAndDeleteTransmission()
    {
        var client = await CreateAuthenticatedClientAsync();
        var transmissionName = $"Transmission-{Guid.NewGuid():N}"[..21];

        var createResponse = await client.PostAsJsonAsync("/api/v1/transmissions", new NamedObjectDto { Name = transmissionName });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTransmission = await createResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        createdTransmission.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/v1/transmissions/{createdTransmission!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedTransmission = await getResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        fetchedTransmission.Should().NotBeNull();
        fetchedTransmission!.Name.Should().Be(transmissionName);

        var updatedName = $"{transmissionName}-A";
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/transmissions/{createdTransmission.Id}", new NamedObjectDto
        {
            Id = createdTransmission.Id,
            Name = updatedName
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getUpdatedResponse = await _client.GetAsync($"/api/v1/transmissions/{createdTransmission.Id}");
        getUpdatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTransmission = await getUpdatedResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        updatedTransmission.Should().NotBeNull();
        updatedTransmission!.Name.Should().Be(updatedName);

        var deleteResponse = await client.DeleteAsync($"/api/v1/transmissions/{createdTransmission.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync("/api/v1/transmissions");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var transmissions = await listResponse.Content.ReadFromJsonAsync<IEnumerable<NamedObjectDto>>();
        transmissions.Should().NotBeNull();
        transmissions!.Should().NotContain(transmission => transmission.Id == createdTransmission.Id);
    }

    [Fact]
    public async Task AddTransmission_ShouldReturnConflict_WhenNameDiffersOnlyByCase()
    {
        var client = await CreateAuthenticatedClientAsync();
        var transmissionName = $"Transmission-{Guid.NewGuid():N}"[..21].ToUpperInvariant();

        var firstResponse = await client.PostAsJsonAsync("/api/v1/transmissions", new NamedObjectDto { Name = transmissionName });
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/transmissions", new NamedObjectDto { Name = transmissionName.ToLowerInvariant() });
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
