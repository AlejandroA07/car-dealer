
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Features.FuelTypes.Commands.Create;
using WestcoastCars.Application.Features.FuelTypes.Commands.Delete;
using WestcoastCars.Application.Features.FuelTypes.Commands.Update;
using WestcoastCars.Application.Features.FuelTypes.Queries.GetById;
using WestcoastCars.Application.Features.FuelTypes.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Threading.Tasks;

namespace WestcoastCars.Api.Controllers
{
    [ApiController]
    [Route("api/v1/fueltypes")]
    public class FuelTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FuelTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListAll()
        {
            var result = await _mediator.Send(new ListAllFuelTypesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetFuelTypeByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            var command = new CreateFuelTypeCommand { Name = model.Name };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] NamedObjectDto model)
        {
            if (id != model.Id)
            {
                return BadRequest("ID mismatch");
            }
            var command = new UpdateFuelTypeCommand { Id = id, Name = model.Name };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteFuelTypeCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
