
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WestcoastCars.Application.Features.Manufacturers.Commands.Create;
using WestcoastCars.Application.Features.Manufacturers.Commands.Delete;
using WestcoastCars.Application.Features.Manufacturers.Commands.Update;
using WestcoastCars.Application.Features.Manufacturers.Queries.GetById;
using WestcoastCars.Application.Features.Manufacturers.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Threading.Tasks;

namespace WestcoastCars.Api.Controllers;

/// <summary>
/// CRUD operations for car manufacturers.
/// </summary>
[ApiController]
[Route("api/v1/manufacturers")]
[Tags("Manufacturers")]
public class ManufacturersController(IMediator mediator, IMemoryCache cache) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IMemoryCache _cache = cache;
    private const string CacheKey = LookupCacheKeys.Manufacturers;

    /// <summary>
    /// Lists all manufacturers.
    /// </summary>
    /// <returns>A collection of manufacturers.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<NamedObjectDto>), 200)]
    public async Task<IActionResult> ListAll()
    {
        if (!_cache.TryGetValue(CacheKey, out IEnumerable<NamedObjectDto>? cached))
        {
            cached = await _mediator.Send(new ListAllManufacturersQuery());
            _cache.Set(CacheKey, cached, TimeSpan.FromMinutes(10));
        }
        return Ok(cached);
    }

    /// <summary>
    /// Retrieves a manufacturer by ID.
    /// </summary>
    /// <param name="id">The manufacturer ID.</param>
    /// <returns>The requested manufacturer.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(NamedObjectDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetManufacturerByIdQuery { Id = id });
        return Ok(result);
    }

    /// <summary>
    /// Creates a new manufacturer. Requires Admin role.
    /// </summary>
    /// <param name="model">Manufacturer data.</param>
    /// <returns>The created manufacturer.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(NamedObjectDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
    {
        var command = new CreateManufacturerCommand { Name = model.Name };
        var result = await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing manufacturer. Requires Admin role.
    /// </summary>
    /// <param name="id">The ID of the manufacturer to update.</param>
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
        var command = new UpdateManufacturerCommand { Id = id, Name = model.Name };
        await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return NoContent();
    }

    /// <summary>
    /// Deletes a manufacturer. Requires Admin role.
    /// </summary>
    /// <param name="id">The ID of the manufacturer to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteManufacturerCommand { Id = id };
        await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return NoContent();
    }
}
