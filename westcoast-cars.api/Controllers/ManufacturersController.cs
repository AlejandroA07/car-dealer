
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Features.Manufacturers.Commands.Create;
using WestcoastCars.Application.Features.Manufacturers.Commands.Delete;
using WestcoastCars.Application.Features.Manufacturers.Commands.Update;
using WestcoastCars.Application.Features.Manufacturers.Queries.GetById;
using WestcoastCars.Application.Features.Manufacturers.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Threading.Tasks;

namespace WestcoastCars.Api.Controllers
{
    [ApiController]
    [Route("api/v1/manufacturers")]
    public class ManufacturersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ManufacturersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListAll()
        {
            var result = await _mediator.Send(new ListAllManufacturersQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetManufacturerByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            var command = new CreateManufacturerCommand { Name = model.Name };
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
            var command = new UpdateManufacturerCommand { Id = id, Name = model.Name };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteManufacturerCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}