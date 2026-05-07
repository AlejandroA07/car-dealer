using System;
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
        var vehicles = await response.Content.ReadFromJsonAsync<PagedResult<VehicleSummaryDto>>();
        vehicles.Should().NotBeNull();
        vehicles!.Items.Should().NotBeEmpty();
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

    [Fact]
    public async Task Search_ShouldBeCaseInsensitiveForMake()
    {
        var response = await _client.GetAsync("/api/v1/vehicles/search?make=volvo&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<VehicleSummaryDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(vehicle => vehicle.Manufacturer.Equals("VOLVO", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListAll_ShouldExcludeSoldVehicles()
    {
        var client = await CreateAuthenticatedClientAsync();

        var manufacturers = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/manufacturers");
        var fuelTypes = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/fueltypes");
        var transmissions = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/transmissions");

        var soldVehicle = new CreateVehicleCommand
        {
            RegistrationNumber = "SOLD123",
            ManufacturerId = manufacturers!.First().Id,
            Model = "Sold Model",
            ModelYear = "2024",
            Mileage = 100,
            FuelTypeId = fuelTypes!.First().Id,
            TransmissionTypeId = transmissions!.First().Id,
            Value = 500000,
            Description = "Sold integration test vehicle",
            IsSold = true
        };

        var createResponse = await client.PostAsJsonAsync("/api/v1/vehicles", soldVehicle);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await _client.GetAsync("/api/v1/vehicles/list?page=1&pageSize=100");

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await listResponse.Content.ReadFromJsonAsync<PagedResult<VehicleSummaryDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotContain(vehicle => vehicle.Name.Contains("Sold Model"));
    }

    [Fact]
    public async Task AddVehicle_ShouldReturnConflict_WhenRegistrationNumberAlreadyExists()
    {
        if (_factory.UsesSqliteFallback)
        {
            return;
        }

        var client = await CreateAuthenticatedClientAsync();

        var manufacturers = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/manufacturers");
        var fuelTypes = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/fueltypes");
        var transmissions = await _client.GetFromJsonAsync<IEnumerable<NamedObjectDto>>("/api/v1/transmissions");
        var registrationNumber = $"DUP{Guid.NewGuid():N}"[..9].ToUpperInvariant();

        var command = new CreateVehicleCommand
        {
            RegistrationNumber = registrationNumber,
            ManufacturerId = manufacturers!.First().Id,
            Model = "V60",
            ModelYear = "2024",
            Mileage = 100,
            FuelTypeId = fuelTypes!.First().Id,
            TransmissionTypeId = transmissions!.First().Id,
            Value = 500000,
            Description = "Integration Test Vehicle"
        };

        var firstResponse = await client.PostAsJsonAsync("/api/v1/vehicles", command);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicateCommand = new CreateVehicleCommand
        {
            RegistrationNumber = registrationNumber.ToLowerInvariant(),
            ManufacturerId = command.ManufacturerId,
            Model = command.Model,
            ModelYear = command.ModelYear,
            Mileage = command.Mileage,
            FuelTypeId = command.FuelTypeId,
            TransmissionTypeId = command.TransmissionTypeId,
            Value = command.Value,
            Description = command.Description
        };

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/vehicles", duplicateCommand);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddManufacturer_ShouldReturnConflict_WhenNameDiffersOnlyByCase()
    {
        if (_factory.UsesSqliteFallback)
        {
            return;
        }

        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/manufacturers", new NamedObjectDto { Name = "volvo" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddFuelType_ShouldReturnConflict_WhenNameDiffersOnlyByCase()
    {
        if (_factory.UsesSqliteFallback)
        {
            return;
        }

        var client = await CreateAuthenticatedClientAsync();
        var fuelTypeName = $"Fuel-{Guid.NewGuid():N}"[..13].ToUpperInvariant();

        var firstResponse = await client.PostAsJsonAsync("/api/v1/fueltypes", new NamedObjectDto { Name = fuelTypeName });
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/fueltypes", new NamedObjectDto { Name = fuelTypeName.ToLowerInvariant() });
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddTransmission_ShouldReturnConflict_WhenNameDiffersOnlyByCase()
    {
        if (_factory.UsesSqliteFallback)
        {
            return;
        }

        var client = await CreateAuthenticatedClientAsync();
        var transmissionName = $"Transmission-{Guid.NewGuid():N}"[..21].ToUpperInvariant();

        var firstResponse = await client.PostAsJsonAsync("/api/v1/transmissions", new NamedObjectDto { Name = transmissionName });
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/transmissions", new NamedObjectDto { Name = transmissionName.ToLowerInvariant() });
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
