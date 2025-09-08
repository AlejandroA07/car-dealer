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
    [Route("api/v1/manufacturers")]
    public class ManufacturersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ManufacturersController> _logger;

        public ManufacturersController(IUnitOfWork unitOfWork, ILogger<ManufacturersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            try
            {
                _logger.LogInformation("Retrieving all manufacturers");
                var manufacturers = await _unitOfWork.Manufacturers.ListAllAsync();
                var result = manufacturers.Select(m => new { Id = m.Id, Name = m.Name }).ToList();
                _logger.LogInformation("Successfully retrieved {Count} manufacturers", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all manufacturers");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving manufacturer with ID: {Id}", id);
                var manufacturer = await _unitOfWork.Manufacturers.FindByIdAsync(id);
                if (manufacturer is null)
                {
                    _logger.LogWarning("Manufacturer with ID {Id} not found", id);
                    return NotFound($"Manufacturer with ID {id} not found");
                }
                _logger.LogInformation("Successfully retrieved manufacturer with ID {Id}", id);
                return Ok(manufacturer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manufacturer with ID: {Id}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            try
            {
                _logger.LogInformation("Attempting to add new manufacturer: {Name}", model.Name);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for new manufacturer.");
                    return BadRequest(ModelState);
                }

                var existing = await _unitOfWork.Manufacturers.ListAsync(m => m.Name.ToUpper() == model.Name.ToUpper());
                if (existing.Any())
                {
                    _logger.LogWarning("Manufacturer '{Name}' already exists.", model.Name);
                    return Conflict($"Manufacturer '{model.Name}' already exists.");
                }

                var manufacturerToAdd = new Manufacturer { Name = model.Name };
                _unitOfWork.Manufacturers.Add(manufacturerToAdd);

                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    _logger.LogInformation("Successfully added new manufacturer with ID {Id}", manufacturerToAdd.Id);
                    return CreatedAtAction(nameof(GetById), new { id = manufacturerToAdd.Id }, manufacturerToAdd);
                }

                _logger.LogError("Failed to add manufacturer '{Name}' to the database.", model.Name);
                return StatusCode(500, "Failed to save manufacturer.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while adding manufacturer: {Name}", model.Name);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NamedObjectDto model)
        {
            try
            {
                _logger.LogInformation("Attempting to update manufacturer with ID: {Id}", id);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for updating manufacturer.");
                    return BadRequest(ModelState);
                }

                var manufacturerToUpdate = await _unitOfWork.Manufacturers.FindByIdAsync(id);
                if (manufacturerToUpdate is null)
                {
                    _logger.LogWarning("Manufacturer with ID {Id} not found for update.", id);
                    return NotFound($"Manufacturer with ID {id} not found.");
                }

                var existing = await _unitOfWork.Manufacturers.ListAsync(m => m.Name.ToUpper() == model.Name.ToUpper() && m.Id != id);
                if (existing.Any())
                {
                    _logger.LogWarning("Manufacturer name '{Name}' already exists on another entity.", model.Name);
                    return Conflict($"Manufacturer name '{model.Name}' already exists.");
                }

                manufacturerToUpdate.Name = model.Name;
                _unitOfWork.Manufacturers.Update(manufacturerToUpdate);

                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    _logger.LogInformation("Successfully updated manufacturer with ID {Id}", id);
                    return NoContent();
                }

                _logger.LogError("Failed to update manufacturer with ID {Id}", id);
                return StatusCode(500, "Failed to update manufacturer.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating manufacturer with ID: {Id}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete manufacturer with ID: {Id}", id);
                var manufacturerToDelete = await _unitOfWork.Manufacturers.FindByIdAsync(id);
                if (manufacturerToDelete is null)
                {
                    _logger.LogWarning("Manufacturer with ID {Id} not found for deletion.", id);
                    return NotFound($"Manufacturer with ID {id} not found.");
                }

                _unitOfWork.Manufacturers.Delete(id);

                if (await _unitOfWork.CompleteAsync() > 0)
                {
                    _logger.LogInformation("Successfully deleted manufacturer with ID {Id}", id);
                    return NoContent();
                }

                _logger.LogError("Failed to delete manufacturer with ID {Id}", id);
                return StatusCode(500, "Failed to delete manufacturer.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting manufacturer with ID: {Id}", id);
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}