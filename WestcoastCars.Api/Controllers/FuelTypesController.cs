
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WestcoastCars.Application.Features.FuelTypes.Commands.Create;
using WestcoastCars.Application.Features.FuelTypes.Commands.Delete;
using WestcoastCars.Application.Features.FuelTypes.Commands.Update;
using WestcoastCars.Application.Features.FuelTypes.Queries.GetById;
using WestcoastCars.Application.Features.FuelTypes.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Threading.Tasks;

namespace WestcoastCars.Api.Controllers;

/// <summary>
/// CRUD operations for fuel types.
/// </summary>
[ApiController]
[Route("api/v1/fueltypes")]
[Tags("Fuel Types")]
public class FuelTypesController(IMediator mediator, IMemoryCache cache) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IMemoryCache _cache = cache;
    private const string CacheKey = "lookup:fueltypes";

    /// <summary>
    /// Lists all fuel types.
    /// </summary>
    /// <returns>A collection of fuel types.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<NamedObjectDto>), 200)]
    public async Task<IActionResult> ListAll()
    {
        if (!_cache.TryGetValue(CacheKey, out IEnumerable<NamedObjectDto> cached))
        {
            cached = await _mediator.Send(new ListAllFuelTypesQuery());
            _cache.Set(CacheKey, cached, TimeSpan.FromMinutes(10));
        }
        return Ok(cached);
    }

    /// <summary>
    /// Retrieves a fuel type by ID.
    /// </summary>
    /// <param name="id">The fuel type ID.</param>
    /// <returns>The requested fuel type.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(NamedObjectDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetFuelTypeByIdQuery { Id = id });
        return Ok(result);
    }

    /// <summary>
    /// Creates a new fuel type. Requires Admin role.
    /// </summary>
    /// <param name="model">Fuel type data.</param>
    /// <returns>The created fuel type.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(NamedObjectDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
    {
        var command = new CreateFuelTypeCommand { Name = model.Name };
        var result = await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing fuel type. Requires Admin role.
    /// </summary>
    /// <param name="id">The ID of the fuel type to update.</param>
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
        var command = new UpdateFuelTypeCommand { Id = id, Name = model.Name };
        await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return NoContent();
    }

    /// <summary>
    /// Deletes a fuel type. Requires Admin role.
    /// </summary>
    /// <param name="id">The ID of the fuel type to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteFuelTypeCommand { Id = id };
        await _mediator.Send(command);
        _cache.Remove(CacheKey);
        return NoContent();
    }
}
