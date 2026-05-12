
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
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VehiclesController> _logger;
    private readonly AppTelemetry _telemetry;
    private readonly IMemoryCache _cache;

    public VehiclesController(IMediator mediator, ILogger<VehiclesController> logger, AppTelemetry telemetry, IMemoryCache cache)
    {
        _mediator = mediator;
        _logger = logger;
        _telemetry = telemetry;
        _cache = cache;
    }

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
            ImageUrl = dto.ImageUrl
        };

        _logger.LogInformation("Creating new vehicle with registration: {RegNo} via MediatR", command.RegistrationNumber);

        using var activity = _telemetry.StartVehicleActivity("create", command.RegistrationNumber);
        var startedAt = Stopwatch.StartNew();

        try
        {
            var result = await _mediator.Send(command);
            startedAt.Stop();
            _telemetry.RecordVehicleOperation("create", "success", startedAt.Elapsed);
            activity?.SetTag("vehicle.id", result.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch
        {
            startedAt.Stop();
            _telemetry.RecordVehicleOperation("create", "failure", startedAt.Elapsed);
            throw;
        }
    }

    /// <summary>
    /// Manually syncs the latest Blocket vehicle listings. Requires Admin or Salesperson role.
    /// </summary>
    /// <param name="command">Sync options such as limit, org id, locations, and models.</param>
    /// <returns>A summary of the sync result.</returns>
    [HttpPost("import/blocket")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(typeof(RefreshInventoryFromBlocketResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> SyncBlocket([FromBody] RefreshInventoryFromBlocketCommand command)
    {
        var request = command ?? new RefreshInventoryFromBlocketCommand();
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
            _cache.Remove("lookup:manufacturers");
            _cache.Remove("lookup:fueltypes");
            _cache.Remove("lookup:transmissions");
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
        var result = await _mediator.Send(new GetVehicleStatsByModelQuery());
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
        var result = await _mediator.Send(new GetVehicleStatsByMileageQuery());
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
        var result = await _mediator.Send(new GetVehicleStatsSummaryQuery());
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
    public async Task<IActionResult> BulkDelete([FromQuery] string? model, [FromQuery] bool? isSold, [FromQuery] int? minMileage, [FromQuery] int? maxMileage)
    {
        if (model is null && isSold is null && minMileage is null && maxMileage is null)
            return BadRequest("At least one filter must be specified.");

        var result = await _mediator.Send(new BulkDeleteVehiclesCommand
        {
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
            ImageUrl = dto.ImageUrl
        };

        _logger.LogInformation("Updating vehicle {Id} via MediatR", id);
        using var activity = _telemetry.StartVehicleActivity("update", command.RegistrationNumber, id);
        var startedAt = Stopwatch.StartNew();

        try
        {
            await _mediator.Send(command);
            startedAt.Stop();
            _telemetry.RecordVehicleOperation("update", "success", startedAt.Elapsed);
            return NoContent();
        }
        catch
        {
            startedAt.Stop();
            _telemetry.RecordVehicleOperation("update", "failure", startedAt.Elapsed);
            throw;
        }
    }

    /// <summary>
    /// Marks a vehicle as sold. Requires Admin or Salesperson role.
    /// </summary>
    /// <param name="id">The ID of the vehicle.</param>
    /// <returns>No content.</returns>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Salesperson")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarkAsSold(int id)
    {
        _logger.LogInformation("Marking vehicle {Id} as sold via MediatR", id);
        using var activity = _telemetry.StartVehicleActivity("mark-as-sold", vehicleId: id);
        var startedAt = Stopwatch.StartNew();

        try
        {
            await _mediator.Send(new MarkAsSoldCommand { Id = id });
            startedAt.Stop();
            _telemetry.RecordVehicleOperation("mark-as-sold", "success", startedAt.Elapsed);
            return NoContent();
        }
        catch
        {
            startedAt.Stop();
            _telemetry.RecordVehicleOperation("mark-as-sold", "failure", startedAt.Elapsed);
            throw;
        }
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
        using var activity = _telemetry.StartVehicleActivity("delete", vehicleId: id);
        var startedAt = Stopwatch.StartNew();

        try
        {
            await _mediator.Send(new DeleteVehicleCommand { Id = id });
            startedAt.Stop();
            _telemetry.RecordVehicleOperation("delete", "success", startedAt.Elapsed);
            return NoContent();
        }
        catch
        {
            startedAt.Stop();
            _telemetry.RecordVehicleOperation("delete", "failure", startedAt.Elapsed);
            throw;
        }
    }
}
