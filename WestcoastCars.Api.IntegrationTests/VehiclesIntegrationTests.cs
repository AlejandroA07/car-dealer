using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using Xunit;

namespace WestcoastCars.Api.IntegrationTests;

public class VehiclesIntegrationTests : IntegrationTestBase
{
    public VehiclesIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ListAll_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/vehicles/list");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var vehicles = await response.Content.ReadFromJsonAsync<IEnumerable<VehicleSummaryDto>>();
        vehicles.Should().NotBeNull();
    }

    [Fact]
    public async Task AddVehicle_ShouldReturnCreated_WhenUserIsAdmin()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        var manufacturers = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/manufacturers");
        var fuelTypes = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/fueltypes");
        var transmissions = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/transmissions");

        var command = new CreateVehicleCommand
        {
            RegistrationNumber = "INTEG123",
            ManufacturerId = manufacturers!.First().Id,
            Model = "V60",
            ModelYear = "2024",
            Mileage = 100,
            FuelTypeId = fuelTypes!.First().Id,
            TransmissionTypeId = transmissions!.First().Id,
            Value = 500000,
            Description = "Integration Test Vehicle"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/vehicles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdVehicle = await response.Content.ReadFromJsonAsync<VehicleDetailsDto>();
        createdVehicle.Should().NotBeNull();
        createdVehicle!.RegistrationNumber.Should().Be("INTEG123");
    }

    [Fact]
    public async Task AddVehicle_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        // Arrange
        var command = new CreateVehicleCommand { RegistrationNumber = "NOTOKEN" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/vehicles", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
