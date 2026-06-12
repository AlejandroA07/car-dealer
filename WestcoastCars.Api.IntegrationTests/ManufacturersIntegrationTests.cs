using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Api.IntegrationTests;

public class ManufacturersIntegrationTests(CustomWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AddManufacturer_ShouldReturnCreated_WhenUserIsAdmin()
    {
        var client = await CreateAuthenticatedClientAsync();
        var manufacturerName = $"Maker-{Guid.NewGuid():N}"[..14];

        var response = await client.PostAsJsonAsync("/api/v1/manufacturers", new NamedObjectDto { Name = manufacturerName });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdManufacturer = await response.Content.ReadFromJsonAsync<NamedObjectDto>();
        createdManufacturer.Should().NotBeNull();
        createdManufacturer!.Name.Should().Be(manufacturerName);
        createdManufacturer.Id.Should().BePositive();
    }

    [Fact]
    public async Task GetManufacturerById_ShouldReturnCreatedManufacturer()
    {
        var client = await CreateAuthenticatedClientAsync();
        var manufacturerName = $"Maker-{Guid.NewGuid():N}"[..14];

        var createResponse = await client.PostAsJsonAsync("/api/v1/manufacturers", new NamedObjectDto { Name = manufacturerName });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdManufacturer = await createResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        createdManufacturer.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/v1/manufacturers/{createdManufacturer!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var manufacturer = await getResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        manufacturer.Should().NotBeNull();
        manufacturer!.Id.Should().Be(createdManufacturer.Id);
        manufacturer.Name.Should().Be(manufacturerName);
    }

    [Fact]
    public async Task DeleteManufacturer_ShouldReturnConflict_WhenVehiclesAreAssigned()
    {
        var client = await CreateAuthenticatedClientAsync();
        var manufacturerResponse = await client.PostAsJsonAsync("/api/v1/manufacturers", new NamedObjectDto
        {
            Name = $"Protected-{Guid.NewGuid():N}"[..18]
        });
        manufacturerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var manufacturer = await manufacturerResponse.Content.ReadFromJsonAsync<NamedObjectDto>();
        manufacturer.Should().NotBeNull();

        var (_, fuelTypeId, transmissionTypeId) = await GetVehicleLookupIdsAsync();
        var vehicleResponse = await client.PostAsJsonAsync("/api/v1/vehicles", new CreateVehicleCommand
        {
            RegistrationNumber = $"MFG{Guid.NewGuid():N}"[..8].ToUpperInvariant(),
            ManufacturerId = manufacturer!.Id,
            Model = "Constraint Model",
            ModelYear = 2024,
            Mileage = 100,
            FuelTypeId = fuelTypeId,
            TransmissionTypeId = transmissionTypeId,
            Price = 500000,
            Description = "Constraint integration test vehicle"
        });
        vehicleResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdVehicle = await vehicleResponse.Content.ReadFromJsonAsync<VehicleDetailsDto>();
        createdVehicle.Should().NotBeNull();

        var response = await client.DeleteAsync($"/api/v1/manufacturers/{manufacturer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var getVehicleResponse = await _client.GetAsync($"/api/v1/vehicles/{createdVehicle!.Id}");
        getVehicleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
