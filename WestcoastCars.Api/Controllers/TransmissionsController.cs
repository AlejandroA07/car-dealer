
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WestcoastCars.Application.Features.Transmissions.Commands.Create;
using WestcoastCars.Application.Features.Transmissions.Commands.Delete;
using WestcoastCars.Application.Features.Transmissions.Commands.Update;
using WestcoastCars.Application.Features.Transmissions.Queries.GetById;
using WestcoastCars.Application.Features.Transmissions.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Threading.Tasks;

namespace WestcoastCars.Api.Controllers;

/// <summary>
/// CRUD operations for transmission types.
/// </summary>
[ApiController]
[Route("api/v1/transmissions")]
[Tags("Transmissions")]
public class TransmissionsController(IMediator mediator, IMemoryCache cache) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IMemoryCache _cache = cache;
    private const string CacheKey = LookupCacheKeys.Transmissions;

    /// <summary>
    /// Lists all transmission types.
    /// </summary>
    /// <returns>A collection of transmission types.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<NamedObjectDto>), 200)]
    public async Task<IActionResult> ListAll()
    {
        var cached = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _mediator.Send(new ListAllTransmissionsQuery());
        });
        return Ok(cached);
    }

    /// <summary>
    /// Retrieves a transmission type by ID.
    /// </summary>
    /// <param name="id">The transmission type ID.</param>
    /// <returns>The requested transmission type.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(NamedObjectDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetTransmissionByIdQuery { Id = id });
        return Ok(result);
    }

    /// <summary>
    /// Creates a new transmission type. Requires Admin role.
    /// </summary>
    /// <param name="model">Transmission type data.</param>
    /// <returns>The created transmission type.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(NamedObjectDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
    {
        var command = new CreateTransmissionCommand { Name = model.Name };
        var result = await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing transmission type. Requires Admin role.
    /// </summary>
    /// <param name="id">The ID of the transmission type to update.</param>
    /// <param name="model">The update data.</param>
    /// <returns>No content.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] NamedObjectDto model)
    {
        if (id != model.Id)
        {
            return BadRequest("ID mismatch");
        }
        var command = new UpdateTransmissionCommand { Id = id, Name = model.Name };
        await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return NoContent();
    }

    /// <summary>
    /// Deletes a transmission type. Requires Admin role.
    /// </summary>
    /// <param name="id">The ID of the transmission type to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteTransmissionCommand { Id = id };
        await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return NoContent();
    }
}
