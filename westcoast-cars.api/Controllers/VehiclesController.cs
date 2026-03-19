
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
using Microsoft.Extensions.Logging;

namespace WestcoastCars.Api.Controllers
{
    [ApiController]
    [Route("api/v1/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IMediator mediator, ILogger<VehiclesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Retrieving list of unsold vehicles via MediatR");
            var result = await _mediator.Send(new ListAllVehiclesQuery());
            return Ok(result);
        }

        [HttpGet("list-all")]
        [Authorize(Roles = "Admin,Salesperson")]
        public async Task<IActionResult> ListAllIncludingSold()
        {
            _logger.LogInformation("Retrieving list of ALL vehicles (including sold) via MediatR");
            var result = await _mediator.Send(new ListAllVehiclesIncludingSoldQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving vehicle with ID: {Id} via MediatR", id);
            var result = await _mediator.Send(new GetVehicleByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpGet("regno/{regNo}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRegNo(string regNo)
        {
            _logger.LogInformation("Retrieving vehicle with registration number: {RegNo} via MediatR", regNo);
            var result = await _mediator.Send(new GetVehicleByRegNoQuery { RegistrationNumber = regNo });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Salesperson")]
        public async Task<IActionResult> Add(CreateVehicleCommand command)
        {
            _logger.LogInformation("🚗 Creating new vehicle with registration: {RegNo} via MediatR", command.RegistrationNumber);
            var id = await _mediator.Send(command);
            var result = await _mediator.Send(new GetVehicleByIdQuery { Id = id });
            return CreatedAtAction(nameof(GetById), new { id = id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Salesperson")]
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

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Salesperson")]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            _logger.LogInformation("Marking vehicle {Id} as sold via MediatR", id);
            await _mediator.Send(new MarkAsSoldCommand { Id = id });
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting vehicle {Id} via MediatR", id);
            await _mediator.Send(new DeleteVehicleCommand { Id = id });
            return NoContent();
        }
    }
}
