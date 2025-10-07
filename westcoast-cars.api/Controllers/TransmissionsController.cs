
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Features.Transmissions.Commands.Create;
using WestcoastCars.Application.Features.Transmissions.Commands.Delete;
using WestcoastCars.Application.Features.Transmissions.Commands.Update;
using WestcoastCars.Application.Features.Transmissions.Queries.GetById;
using WestcoastCars.Application.Features.Transmissions.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Threading.Tasks;

namespace WestcoastCars.Api.Controllers
{
    [ApiController]
    [Route("api/v1/transmissions")]
    public class TransmissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransmissionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListAll()
        {
            var result = await _mediator.Send(new ListAllTransmissionsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetTransmissionByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            var command = new CreateTransmissionCommand { Name = model.Name };
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
            var command = new UpdateTransmissionCommand { Id = id, Name = model.Name };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteTransmissionCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
