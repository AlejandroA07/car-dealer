
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Application.Features.Vehicles.Queries.ListAll;
using WestcoastCars.Application.Features.Vehicles.Queries.GetById;
using WestcoastCars.Application.Features.Vehicles.Queries.GetByRegNo;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using WestcoastCars.Application.Features.Vehicles.Commands.Update;
using WestcoastCars.Application.Features.Vehicles.Commands.Delete;
using WestcoastCars.Application.Features.Vehicles.Commands.MarkAsSold;
using WestcoastCars.Application.Features.Vehicles.Commands.RefreshInventoryFromBlocket;
using WestcoastCars.Application.Features.Vehicles.Commands.PurgeSourceRemoved;
using WestcoastCars.Application.Features.Vehicles.Queries.PreviewBlocketVehicles;
using WestcoastCars.Application.Features.Vehicles.Commands.ImportSelectedBlocketVehicles;
using WestcoastCars.Application.Features.Vehicles.Commands.BulkDelete;
using WestcoastCars.Application.Features.Vehicles.Commands.DeleteAll;
using WestcoastCars.Application.Features.Vehicles.Queries.Stats;
using WestcoastCars.Application.Features.Vehicles.Queries.Search;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WestcoastCars.Api.Observability;
using System.Diagnostics;

namespace WestcoastCars.Api.Controllers;

/// <summary>
/// Manages vehicle inventory operations.
/// </summary>
[ApiController]
[Route("api/v1/vehicles")]
[Tags("Vehicles")]
public class VehiclesController(IMediator mediator, ILogger<VehiclesController> logger, AppTelemetry telemetry, IMemoryCache cache) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<VehiclesController> _logger = logger;
    private readonly AppTelemetry _telemetry = telemetry;
    private readonly IMemoryCache _cache = cache;

    /// <summary>
    /// Searches vehicles by various criteria.
    /// </summary>
    /// <param name="search">Search parameters including make, year range, price range.</param>
    /// <returns>List of vehicles matching the search criteria.</returns>
    /// <response code="200">Returns matching vehicles.</response>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<VehicleSummaryDto>), 200)]
    public async Task<IActionResult> Search([FromQuery] VehicleSearchDto search)
    {
        _logger.LogInformation("Searching vehicles with criteria: {@Search}", search);
        var result = await _mediator.Send(new SearchVehiclesQuery(search));
        return Ok(result);
    }

    /// <summary>
    /// Lists all unsold vehicles.
    /// </summary>
    /// <returns>A collection of unsold vehicles.</returns>
    [HttpGet("list")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<VehicleSummaryDto>), 200)]
    public async Task<IActionResult> ListAll([FromQuery] PagedQueryDto pagination)
    {
        _logger.LogInformation("Retrieving list of unsold vehicles via MediatR");
        var result = await _mediator.Send(new ListAllVehiclesQuery
        {
            Page = pagination.Page,
            PageSize = pagination.PageSize
        });
        return Ok(result);
    }

    /// <summary>
    /// Lists all vehicles including sold ones. Requires Admin or Salesperson role.
    /// </summary>
    /// <returns>A collection of all vehicles.</returns>
    [HttpGet("list-all")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(PagedResult<VehicleSummaryDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ListAllIncludingSold([FromQuery] PagedQueryDto pagination)
    {
        _logger.LogInformation("Retrieving list of ALL vehicles (including sold) via MediatR");
        var result = await _mediator.Send(new ListAllVehiclesIncludingSoldQuery
        {
            Page = pagination.Page,
            PageSize = pagination.PageSize
        });
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a vehicle by its ID.
    /// </summary>
    /// <param name="id">The vehicle ID.</param>
    /// <returns>The requested vehicle.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VehicleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Retrieving vehicle with ID: {Id} via MediatR", id);
        var result = await _mediator.Send(new GetVehicleByIdQuery { Id = id });
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a vehicle by its registration number.
    /// </summary>
    /// <param name="regNo">The registration number.</param>
    /// <returns>The requested vehicle.</returns>
    [HttpGet("regno/{regNo}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VehicleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByRegNo(string regNo)
    {
        _logger.LogInformation("Retrieving vehicle with registration number: {RegNo} via MediatR", regNo);
        var result = await _mediator.Send(new GetVehicleByRegNoQuery { RegistrationNumber = regNo });
        return Ok(result);
    }

    /// <summary>
    /// Creates a new vehicle listing. Requires Admin or Salesperson role.
    /// </summary>
    /// <param name="dto">Vehicle creation data.</param>
    /// <returns>The created vehicle.</returns>
    /// <response code="201">Vehicle created successfully.</response>
    /// <response code="400">Invalid vehicle data.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User not authorized (requires Admin or Salesperson role).</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(VehicleDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Add(VehiclePostDto dto)
    {
        var command = new CreateVehicleCommand
        {
            RegistrationNumber = dto.RegistrationNumber,
            ManufacturerId = dto.ManufacturerId,
            Model = dto.Model,
            ModelYear = dto.ModelYear,
            Mileage = dto.Mileage,
            FuelTypeId = dto.FuelTypeId,
            TransmissionTypeId = dto.TransmissionTypeId,
            Price = dto.Price,
            Description = dto.Description,
            IsSold = dto.IsSold,
            ImageUrl = dto.ImageUrl,
            Color = dto.Color,
            WheelDrive = dto.WheelDrive,
            Horsepower = dto.Horsepower,
            BodyType = dto.BodyType,
            Doors = dto.Doors,
            EngineVolume = dto.EngineVolume,
            City = dto.City,
            Address = dto.Address,
            Seats = dto.Seats,
            MaxTrailerWeight = dto.MaxTrailerWeight,
            OwnerCount = dto.OwnerCount,
            LastInspectionDate = dto.LastInspectionDate,
            NextInspectionDate = dto.NextInspectionDate,
            Equipment = dto.Equipment,
            GalleryUrls = dto.GalleryUrls
        };

        _logger.LogInformation("Creating new vehicle with registration: {RegNo} via MediatR", command.RegistrationNumber);
        return await ExecuteWithVehicleTelemetryAsync("create", async activity =>
        {
            var result = await _mediator.Send(command);
            activity?.SetTag("vehicle.id", result.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }, regNo: command.RegistrationNumber);
    }

    /// <summary>
    /// Manually syncs the latest Blocket vehicle listings. Requires Admin or Salesperson role.
    /// </summary>
    /// <param name="dto">Sync options such as limit, org id, locations, and models.</param>
    /// <returns>A summary of the sync result.</returns>
    [HttpPost("import/blocket")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(RefreshInventoryFromBlocketResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> SyncBlocket([FromBody] BlocketSearchParamsDto dto)
    {
        var request = new RefreshInventoryFromBlocketCommand
        {
            Limit = dto?.Limit ?? 50,
            Query = dto?.Query,
            SortOrder = dto?.SortOrder,
            OrgId = dto?.OrgId,
            Locations = dto?.Locations,
            Manufacturers = dto?.Manufacturers,
            PriceFrom = dto?.PriceFrom,
            PriceTo = dto?.PriceTo,
            YearFrom = dto?.YearFrom,
            YearTo = dto?.YearTo,
            MinMileage = dto?.MinMileage,
            MaxMileage = dto?.MaxMileage,
            Colors = dto?.Colors,
            TransmissionFilter = dto?.TransmissionFilter,
            WheelDrive = dto?.WheelDrive,
            HorsepowerFrom = dto?.HorsepowerFrom,
            HorsepowerTo = dto?.HorsepowerTo,
            FuelTypeFilter = dto?.FuelTypeFilter
        };
        _logger.LogInformation("Starting manual Blocket sync with limit {Limit}", request.Limit);
        using var activity = _telemetry.StartBlocketSyncActivity(request.Limit);
        var startedAt = Stopwatch.StartNew();

        try
        {
            var result = await _mediator.Send(request);
            startedAt.Stop();
            _telemetry.RecordBlocketSync("success", startedAt.Elapsed, request.Limit);
            activity?.SetTag("blocket_sync.total_added", result.TotalAdded);
            activity?.SetTag("blocket_sync.total_updated", result.TotalUpdated);
            activity?.SetTag("blocket_sync.total_flagged", result.TotalFlagged);
            _cache.Remove(LookupCacheKeys.Manufacturers);
            _cache.Remove(LookupCacheKeys.FuelTypes);
            _cache.Remove(LookupCacheKeys.Transmissions);
            return Ok(result);
        }
        catch
        {
            startedAt.Stop();
            _telemetry.RecordBlocketSync("failure", startedAt.Elapsed, request.Limit);
            throw;
        }
    }

    /// <summary>
    /// Previews Blocket search results without importing. Requires Admin or Salesperson role.
    /// </summary>
    [HttpPost("preview/blocket")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(List<BlocketPreviewDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> PreviewBlocket([FromBody] BlocketSearchParamsDto dto)
    {
        var request = new PreviewBlocketVehiclesQuery
        {
            Limit = dto?.Limit ?? 50,
            Query = dto?.Query,
            SortOrder = dto?.SortOrder,
            OrgId = dto?.OrgId,
            Locations = dto?.Locations,
            Manufacturers = dto?.Manufacturers,
            PriceFrom = dto?.PriceFrom,
            PriceTo = dto?.PriceTo,
            YearFrom = dto?.YearFrom,
            YearTo = dto?.YearTo,
            MinMileage = dto?.MinMileage,
            MaxMileage = dto?.MaxMileage,
            Colors = dto?.Colors,
            TransmissionFilter = dto?.TransmissionFilter,
            WheelDrive = dto?.WheelDrive,
            HorsepowerFrom = dto?.HorsepowerFrom,
            HorsepowerTo = dto?.HorsepowerTo,
            FuelTypeFilter = dto?.FuelTypeFilter
        };
        _logger.LogInformation("Previewing Blocket listings with limit {Limit}", request.Limit);
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    /// <summary>
    /// Imports a specific selection of Blocket listings by their external IDs. Requires Admin or Salesperson role.
    /// </summary>
    [HttpPost("import/blocket/selected")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(ImportSelectedResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ImportSelected([FromBody] ImportSelectedRequestDto dto)
    {
        var command = new ImportSelectedBlocketVehiclesCommand
        {
            ExternalListingIds = dto?.ExternalListingIds ?? [],
            ImageUrlsById = dto?.ImageUrlsById ?? []
        };
        _logger.LogInformation("Importing {Count} selected Blocket listings", command.ExternalListingIds.Count);
        var result = await _mediator.Send(command);
        _cache.Remove("lookup:manufacturers");
        _cache.Remove("lookup:fueltypes");
        _cache.Remove("lookup:transmissions");
        return Ok(result);
    }

    /// <summary>
    /// Permanently deletes all Blocket vehicles flagged as SourceRemoved. Requires Admin role.
    /// </summary>
    /// <returns>Count of vehicles deleted.</returns>
    [HttpDelete("import/blocket/removed")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PurgeSourceRemovedVehiclesResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> PurgeSourceRemoved()
    {
        _logger.LogInformation("Purging SourceRemoved Blocket vehicles");
        var result = await _mediator.Send(new PurgeSourceRemovedVehiclesCommand());
        return Ok(result);
    }

    /// <summary>
    /// Returns vehicle counts grouped by model.
    /// </summary>
    [HttpGet("stats/by-model")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(IEnumerable<VehicleStatsByModelDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> StatsByModel()
    {
        var result = await _cache.GetOrCreateAsync(LookupCacheKeys.StatsByModel, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return await _mediator.Send(new GetVehicleStatsByModelQuery());
        });
        return Ok(result);
    }

    /// <summary>
    /// Returns vehicle counts grouped by predefined mileage bands.
    /// </summary>
    [HttpGet("stats/by-mileage")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(IEnumerable<VehicleStatsByMileageDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> StatsByMileage()
    {
        var result = await _cache.GetOrCreateAsync(LookupCacheKeys.StatsByMileage, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return await _mediator.Send(new GetVehicleStatsByMileageQuery());
        });
        return Ok(result);
    }

    /// <summary>
    /// Returns a summary of total, sold, unsold, and source-removed vehicle counts.
    /// </summary>
    [HttpGet("stats/summary")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(VehicleStatsSummaryDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> StatsSummary()
    {
        var result = await _cache.GetOrCreateAsync(LookupCacheKeys.StatsSummary, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return await _mediator.Send(new GetVehicleStatsSummaryQuery());
        });
        return Ok(result);
    }

    /// <summary>
    /// Bulk deletes vehicles matching the given filter criteria. Requires Admin role.
    /// At least one filter must be provided.
    /// </summary>
    [HttpDelete("bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BulkDeleteVehiclesResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> BulkDelete([FromQuery] string make, [FromQuery] string model, [FromQuery] bool? isSold, [FromQuery] int? minMileage, [FromQuery] int? maxMileage)
    {
        var result = await _mediator.Send(new BulkDeleteVehiclesCommand
        {
            Make = make,
            Model = model,
            IsSold = isSold,
            MinMileage = minMileage,
            MaxMileage = maxMileage
        });
        return Ok(result);
    }

    /// <summary>
    /// Permanently deletes all vehicles in the database. Requires Admin role.
    /// </summary>
    [HttpDelete("bulk/all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DeleteAllVehiclesResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> DeleteAll()
    {
        var result = await _mediator.Send(new DeleteAllVehiclesCommand());
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing vehicle. Requires Admin or Salesperson role.
    /// </summary>
    /// <param name="id">The ID of the vehicle to update.</param>
    /// <param name="dto">The update data.</param>
    /// <returns>No content.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateVehicle(int id, VehicleUpdateDto dto)
    {
        if (id != dto.Id)
        {
            _logger.LogWarning("ID mismatch for vehicle update: {Id} vs {CommandId}", id, dto.Id);
            return BadRequest("ID mismatch");
        }

        var command = new UpdateVehicleCommand
        {
            Id = dto.Id,
            RegistrationNumber = dto.RegistrationNumber,
            ManufacturerId = dto.ManufacturerId,
            Model = dto.Model,
            ModelYear = dto.ModelYear,
            Mileage = dto.Mileage,
            FuelTypeId = dto.FuelTypeId,
            TransmissionTypeId = dto.TransmissionTypeId,
            Price = dto.Price,
            Description = dto.Description,
            IsSold = dto.IsSold,
            ImageUrl = dto.ImageUrl,
            Color = dto.Color,
            WheelDrive = dto.WheelDrive,
            Horsepower = dto.Horsepower,
            BodyType = dto.BodyType,
            Doors = dto.Doors,
            EngineVolume = dto.EngineVolume,
            City = dto.City,
            Address = dto.Address,
            Seats = dto.Seats,
            MaxTrailerWeight = dto.MaxTrailerWeight,
            OwnerCount = dto.OwnerCount,
            LastInspectionDate = dto.LastInspectionDate,
            NextInspectionDate = dto.NextInspectionDate,
            Equipment = dto.Equipment,
            GalleryUrls = dto.GalleryUrls
        };

        _logger.LogInformation("Updating vehicle {Id} via MediatR", id);
        return await ExecuteWithVehicleTelemetryAsync("update", async _ =>
        {
            await _mediator.Send(command);
            return NoContent();
        }, regNo: command.RegistrationNumber, vehicleId: id);
    }

    /// <summary>
    /// Marks a vehicle as sold. Requires Admin or Salesperson role.
    /// </summary>
    /// <param name="id">The ID of the vehicle.</param>
    /// <returns>No content.</returns>
    [HttpPatch("{id}/sold")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAsSold(int id)
    {
        _logger.LogInformation("Marking vehicle {Id} as sold via MediatR", id);
        return await ExecuteWithVehicleTelemetryAsync("mark-as-sold", async _ =>
        {
            await _mediator.Send(new MarkAsSoldCommand { Id = id });
            return NoContent();
        }, vehicleId: id);
    }

    /// <summary>
    /// Deletes a vehicle. Requires Admin role.
    /// </summary>
    /// <param name="id">The ID of the vehicle to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deleting vehicle {Id} via MediatR", id);
        return await ExecuteWithVehicleTelemetryAsync("delete", async _ =>
        {
            await _mediator.Send(new DeleteVehicleCommand { Id = id });
            return NoContent();
        }, vehicleId: id);
    }

    private async Task<IActionResult> ExecuteWithVehicleTelemetryAsync(
        string operation,
        Func<Activity?, Task<IActionResult>> action,
        string? regNo = null,
        int? vehicleId = null)
    {
        using var activity = _telemetry.StartVehicleActivity(operation, regNo, vehicleId);
        var startedAt = Stopwatch.StartNew();
        try
        {
            var result = await action(activity);
            startedAt.Stop();
            _telemetry.RecordVehicleOperation(operation, "success", startedAt.Elapsed);
            return result;
        }
        catch
        {
            startedAt.Stop();
            _telemetry.RecordVehicleOperation(operation, "failure", startedAt.Elapsed);
            throw;
        }
    }
}
