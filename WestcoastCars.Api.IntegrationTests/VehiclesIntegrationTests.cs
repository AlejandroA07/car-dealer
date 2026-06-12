using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using Xunit;

namespace WestcoastCars.Api.IntegrationTests;

public class VehiclesIntegrationTests(CustomWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
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
        var (manufacturerId, fuelTypeId, transmissionTypeId) = await GetVehicleLookupIdsAsync();

        var command = new CreateVehicleCommand
        {
            RegistrationNumber = "INTEG123",
            ManufacturerId = manufacturerId,
            Model = "V60",
            ModelYear = 2024,
            Mileage = 100,
            FuelTypeId = fuelTypeId,
            TransmissionTypeId = transmissionTypeId,
            Price = 500000,
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
    public async Task GetByRegNo_ShouldReturnVehicle_WhenVehicleExists()
    {
        var client = await CreateAuthenticatedClientAsync();
        var registrationNumber = $"REG{Guid.NewGuid():N}"[..9].ToUpperInvariant();
        var createdVehicle = await CreateVehicleAsync(client, registrationNumber, "XC60");

        var response = await _client.GetAsync($"/api/v1/vehicles/regno/{registrationNumber}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var vehicle = await response.Content.ReadFromJsonAsync<VehicleDetailsDto>();
        vehicle.Should().NotBeNull();
        vehicle!.Id.Should().Be(createdVehicle.Id);
        vehicle.RegistrationNumber.Should().Be(registrationNumber);
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
        var (manufacturerId, fuelTypeId, transmissionTypeId) = await GetVehicleLookupIdsAsync();

        var soldVehicle = new CreateVehicleCommand
        {
            RegistrationNumber = "SOLD123",
            ManufacturerId = manufacturerId,
            Model = "Sold Model",
            ModelYear = 2024,
            Mileage = 100,
            FuelTypeId = fuelTypeId,
            TransmissionTypeId = transmissionTypeId,
            Price = 500000,
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
        var client = await CreateAuthenticatedClientAsync();
        var (manufacturerId, fuelTypeId, transmissionTypeId) = await GetVehicleLookupIdsAsync();
        var registrationNumber = $"DUP{Guid.NewGuid():N}"[..9].ToUpperInvariant();

        var command = new CreateVehicleCommand
        {
            RegistrationNumber = registrationNumber,
            ManufacturerId = manufacturerId,
            Model = "V60",
            ModelYear = 2024,
            Mileage = 100,
            FuelTypeId = fuelTypeId,
            TransmissionTypeId = transmissionTypeId,
            Price = 500000,
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
            Price = command.Price,
            Description = command.Description
        };

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/vehicles", duplicateCommand);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddVehicle_ShouldReturnBadRequestWithProblemDetails_WhenPayloadIsInvalid()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/vehicles", new VehiclePostDto
        {
            RegistrationNumber = "A",
            ManufacturerId = 0,
            Model = "A",
            ModelYear = 24,
            Mileage = 100,
            FuelTypeId = 0,
            TransmissionTypeId = 0,
            Price = 0,
            Description = "Invalid vehicle payload",
            ImageUrl = "relative-image.png"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions.Should().ContainKey("traceId");

        var errors = problemDetails.Extensions["errors"].Should().BeOfType<JsonElement>().Subject;
        errors.ValueKind.Should().Be(JsonValueKind.Object);
        errors.EnumerateObject().Select(property => property.Name).Should().Contain([
            "ModelYear",
            "Price",
            "ManufacturerId",
            "FuelTypeId",
            "TransmissionTypeId",
            "ImageUrl"
        ]);
    }

    [Fact]
    public async Task AddManufacturer_ShouldReturnConflict_WhenNameDiffersOnlyByCase()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/manufacturers", new NamedObjectDto { Name = "volvo" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
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

    [Fact]
    public async Task VehicleCrudFlow_ShouldCreateFetchUpdateMarkSoldAndDeleteVehicle()
    {
        var client = await CreateAuthenticatedClientAsync();
        var registrationNumber = $"FLOW{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        var createdVehicle = await CreateVehicleAsync(client, registrationNumber, "XC60");

        var getResponse = await _client.GetAsync($"/api/v1/vehicles/{createdVehicle.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedVehicle = await getResponse.Content.ReadFromJsonAsync<VehicleDetailsDto>();
        fetchedVehicle.Should().NotBeNull();
        fetchedVehicle!.RegistrationNumber.Should().Be(registrationNumber);

        var (manufacturerId, fuelTypeId, transmissionTypeId) = await GetVehicleLookupIdsAsync();
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/vehicles/{createdVehicle.Id}", new VehicleUpdateDto
        {
            Id = createdVehicle.Id,
            RegistrationNumber = registrationNumber,
            ManufacturerId = manufacturerId,
            Model = "XC90",
            ModelYear = 2025,
            Mileage = 500,
            FuelTypeId = fuelTypeId,
            TransmissionTypeId = transmissionTypeId,
            Price = 650000,
            Description = "Updated integration flow vehicle",
            IsSold = false,
            ImageUrl = string.Empty
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedFetchResponse = await _client.GetAsync($"/api/v1/vehicles/{createdVehicle.Id}");
        updatedFetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedVehicle = await updatedFetchResponse.Content.ReadFromJsonAsync<VehicleDetailsDto>();
        updatedVehicle.Should().NotBeNull();
        updatedVehicle!.Model.Should().Be("XC90");
        updatedVehicle.Description.Should().Be("Updated integration flow vehicle");

        var markSoldResponse = await client.PatchAsync($"/api/v1/vehicles/{createdVehicle.Id}", new StringContent(string.Empty));
        markSoldResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var unsoldListResponse = await _client.GetAsync("/api/v1/vehicles/list?page=1&pageSize=100");
        unsoldListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var unsoldList = await unsoldListResponse.Content.ReadFromJsonAsync<PagedResult<VehicleSummaryDto>>();
        unsoldList.Should().NotBeNull();
        unsoldList!.Items.Should().NotContain(vehicle => vehicle.Id == createdVehicle.Id);

        var deleteResponse = await client.DeleteAsync($"/api/v1/vehicles/{createdVehicle.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/v1/vehicles/{createdVehicle.Id}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAsSold_ShouldReturnConflict_WhenVehicleIsAlreadySold()
    {
        var client = await CreateAuthenticatedClientAsync();
        var registrationNumber = $"SOLD{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        var createdVehicle = await CreateVehicleAsync(client, registrationNumber, "XC60");

        var firstResponse = await client.PatchAsync($"/api/v1/vehicles/{createdVehicle.Id}", new StringContent(string.Empty));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondResponse = await client.PatchAsync($"/api/v1/vehicles/{createdVehicle.Id}", new StringContent(string.Empty));
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddVehicle_ShouldReturnNotFound_WhenRelatedLookupDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/vehicles", new CreateVehicleCommand
        {
            RegistrationNumber = $"MISS{Guid.NewGuid():N}"[..8].ToUpperInvariant(),
            ManufacturerId = int.MaxValue,
            Model = "V90",
            ModelYear = 2024,
            Mileage = 100,
            FuelTypeId = int.MaxValue,
            TransmissionTypeId = int.MaxValue,
            Price = 500000,
            Description = "Missing related entities"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnForbidden_WhenUserIsSalesperson()
    {
        var adminClient = await CreateAuthenticatedClientAsync();
        var salespersonClient = await CreateSalespersonClientAsync();
        var registrationNumber = $"FORBID{Guid.NewGuid():N}"[..10].ToUpperInvariant();
        var createdVehicle = await CreateVehicleAsync(adminClient, registrationNumber, "XC60");

        var response = await salespersonClient.DeleteAsync($"/api/v1/vehicles/{createdVehicle.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
