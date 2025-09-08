using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Contracts.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/fueltypes")]
    public class FuelTypesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FuelTypesController> _logger;

        public FuelTypesController(IUnitOfWork unitOfWork, ILogger<FuelTypesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            try
            {
                _logger.LogInformation("Retrieving all fuel types");
                var fuelTypes = await _unitOfWork.FuelTypes.ListAllAsync();
                var result = fuelTypes.Select(f => new { Id = f.Id, Name = f.Name }).ToList();
                _logger.LogInformation("Successfully retrieved {Count} fuel types", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all fuel types");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving fuel type with ID: {Id}", id);
                var fuelType = await _unitOfWork.FuelTypes.FindByIdAsync(id);
                if (fuelType is null)
                {
                    _logger.LogWarning("Fuel type with ID {Id} not found", id);
                    return NotFound($"Fuel type with ID {id} not found");
                }
                _logger.LogInformation("Successfully retrieved fuel type with ID {Id}", id);
                return Ok(fuelType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fuel type with ID: {Id}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            try
            {
                _logger.LogInformation("Attempting to add new fuel type: {Name}", model.Name);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for new fuel type.");
                    return BadRequest(ModelState);
                }

                var existing = await _unitOfWork.FuelTypes.ListAsync(f => f.Name.ToUpper() == model.Name.ToUpper());
                if (existing.Any())
                {
                    _logger.LogWarning("Fuel type '{Name}' already exists.", model.Name);
                    return Conflict($"Fuel type '{model.Name}' already exists.");
                }

                var fuelTypeToAdd = new FuelType { Name = model.Name };
                _unitOfWork.FuelTypes.Add(fuelTypeToAdd);

                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    _logger.LogInformation("Successfully added new fuel type with ID {Id}", fuelTypeToAdd.Id);
                    return CreatedAtAction(nameof(GetById), new { id = fuelTypeToAdd.Id }, fuelTypeToAdd);
                }

                _logger.LogError("Failed to add fuel type '{Name}' to the database.", model.Name);
                return StatusCode(500, "Failed to save fuel type.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while adding fuel type: {Name}", model.Name);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NamedObjectDto model)
        {
            try
            {
                _logger.LogInformation("Attempting to update fuel type with ID: {Id}", id);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for updating fuel type.");
                    return BadRequest(ModelState);
                }

                var fuelTypeToUpdate = await _unitOfWork.FuelTypes.FindByIdAsync(id);
                if (fuelTypeToUpdate is null)
                {
                    _logger.LogWarning("Fuel type with ID {Id} not found for update.", id);
                    return NotFound($"Fuel type with ID {id} not found.");
                }

                var existing = await _unitOfWork.FuelTypes.ListAsync(f => f.Name.ToUpper() == model.Name.ToUpper() && f.Id != id);
                if (existing.Any())
                {
                    _logger.LogWarning("Fuel type name '{Name}' already exists on another entity.", model.Name);
                    return Conflict($"Fuel type name '{model.Name}' already exists.");
                }

                fuelTypeToUpdate.Name = model.Name;
                _unitOfWork.FuelTypes.Update(fuelTypeToUpdate);

                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    _logger.LogInformation("Successfully updated fuel type with ID {Id}", id);
                    return NoContent();
                }

                _logger.LogError("Failed to update fuel type with ID {Id}", id);
                return StatusCode(500, "Failed to update fuel type.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating fuel type with ID: {Id}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete fuel type with ID: {Id}", id);
                var fuelTypeToDelete = await _unitOfWork.FuelTypes.FindByIdAsync(id);
                if (fuelTypeToDelete is null)
                {
                    _logger.LogWarning("Fuel type with ID {Id} not found for deletion.", id);
                    return NotFound($"Fuel type with ID {id} not found.");
                }

                _unitOfWork.FuelTypes.Delete(id);

                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    _logger.LogInformation("Successfully deleted fuel type with ID {Id}", id);
                    return NoContent();
                }

                _logger.LogError("Failed to delete fuel type with ID {Id}", id);
                return StatusCode(500, "Failed to delete fuel type.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting fuel type with ID: {Id}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
