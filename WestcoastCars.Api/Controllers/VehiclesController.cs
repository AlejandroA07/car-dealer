
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
using WestcoastCars.Application.Features.Vehicles.Commands.SyncBlocket;
using WestcoastCars.Application.Features.Vehicles.Queries.Search;
using Microsoft.Extensions.Logging;

namespace WestcoastCars.Api.Controllers
{
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

        public VehiclesController(IMediator mediator, ILogger<VehiclesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Searches vehicles by various criteria.
        /// </summary>
        /// <param name="search">Search parameters including make, year range, price range.</param>
        /// <returns>List of vehicles matching the search criteria.</returns>
        /// <response code="200">Returns matching vehicles.</response>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<VehicleSummaryDto>), 200)]
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
        [ProducesResponseType(typeof(IEnumerable<VehicleSummaryDto>), 200)]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Retrieving list of unsold vehicles via MediatR");
            var result = await _mediator.Send(new ListAllVehiclesQuery());
            return Ok(result);
        }

        /// <summary>
        /// Lists all vehicles including sold ones. Requires Admin or Salesperson role.
        /// </summary>
        /// <returns>A collection of all vehicles.</returns>
        [HttpGet("list-all")]
        [Authorize(Roles = "Admin,Salesperson")]
        [ProducesResponseType(typeof(IEnumerable<VehicleSummaryDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> ListAllIncludingSold()
        {
            _logger.LogInformation("Retrieving list of ALL vehicles (including sold) via MediatR");
            var result = await _mediator.Send(new ListAllVehiclesIncludingSoldQuery());
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
        /// <param name="command">Vehicle creation data.</param>
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
        public async Task<IActionResult> Add(CreateVehicleCommand command)
        {
            _logger.LogInformation("🚗 Creating new vehicle with registration: {RegNo} via MediatR", command.RegistrationNumber);
            var id = await _mediator.Send(command);
            var result = await _mediator.Send(new GetVehicleByIdQuery { Id = id });
            return CreatedAtAction(nameof(GetById), new { id = id }, result);
        }

        /// <summary>
        /// Manually syncs the latest Blocket vehicle listings. Requires Admin or Salesperson role.
        /// </summary>
        /// <param name="command">Sync options such as limit, org id, locations, and models.</param>
        /// <returns>A summary of the sync result.</returns>
        [HttpPost("import/blocket")]
        [Authorize(Roles = "Admin,Salesperson")]
        [ProducesResponseType(typeof(SyncBlocketVehiclesResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> SyncBlocket([FromBody] SyncBlocketVehiclesCommand command)
        {
            var request = command ?? new SyncBlocketVehiclesCommand();
            _logger.LogInformation("🔄 Starting manual Blocket sync with limit {Limit}", request.Limit);
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing vehicle. Requires Admin or Salesperson role.
        /// </summary>
        /// <param name="id">The ID of the vehicle to update.</param>
        /// <param name="command">The update data.</param>
        /// <returns>No content.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Salesperson")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleCommand command)
        {
            if (id != command.Id)
            {
                _logger.LogWarning("ID mismatch for vehicle update: {Id} vs {CommandId}", id, command.Id);
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("🔄 Updating vehicle {Id} via MediatR", id);
            await _mediator.Send(command);
            return NoContent();
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
            await _mediator.Send(new MarkAsSoldCommand { Id = id });
            return NoContent();
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
            await _mediator.Send(new DeleteVehicleCommand { Id = id });
            return NoContent();
        }
    }
}
