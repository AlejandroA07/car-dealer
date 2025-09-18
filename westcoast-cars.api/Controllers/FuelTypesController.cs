
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Api.Exceptions;
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
        [AllowAnonymous]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Retrieving all fuel types");
            var fuelTypes = await _unitOfWork.Repository<FuelType>().GetAllAsync();
            var result = fuelTypes.Select(f => new { Id = f.Id, Name = f.Name }).ToList();
            _logger.LogInformation("Successfully retrieved {Count} fuel types", result.Count);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving fuel type with ID: {Id}", id);
            var fuelType = await _unitOfWork.Repository<FuelType>().GetByIdAsync(id);
            if (fuelType is null)
            {
                _logger.LogWarning("Fuel type with ID {Id} not found", id);
                throw new NotFoundException($"Fuel type with ID {id} not found");
            }
            _logger.LogInformation("Successfully retrieved fuel type with ID {Id}", id);
            return Ok(fuelType);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            _logger.LogInformation("Attempting to add new fuel type: {Name}", model.Name);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for new fuel type.");
                return BadRequest(ModelState);
            }

            var existing = await _unitOfWork.Repository<FuelType>().FirstOrDefaultAsync(f => f.Name.ToUpper() == model.Name.ToUpper());
            if (existing != null)
            {
                _logger.LogWarning("Fuel type '{Name}' already exists.", model.Name);
                throw new ConflictException($"Fuel type '{model.Name}' already exists.");
            }

            var fuelTypeToAdd = new FuelType { Name = model.Name };
            await _unitOfWork.Repository<FuelType>().AddAsync(fuelTypeToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Successfully added new fuel type with ID {Id}", fuelTypeToAdd.Id);
                return CreatedAtAction(nameof(GetById), new { id = fuelTypeToAdd.Id }, fuelTypeToAdd);
            }

            _logger.LogError("Failed to add fuel type '{Name}' to the database.", model.Name);
            return StatusCode(500, "Failed to save fuel type.");
        }

        

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Attempting to delete fuel type with ID: {Id}", id);
            var fuelTypeToDelete = await _unitOfWork.Repository<FuelType>().GetByIdAsync(id);
            if (fuelTypeToDelete is null)
            {
                _logger.LogWarning("Fuel type with ID {Id} not found for deletion.", id);
                throw new NotFoundException($"Fuel type with ID {id} not found.");
            }

            _unitOfWork.Repository<FuelType>().Remove(fuelTypeToDelete);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Successfully deleted fuel type with ID {Id}", id);
                return NoContent();
            }

            _logger.LogError("Failed to delete fuel type with ID {Id}", id);
            return StatusCode(500, "Failed to delete fuel type.");
        }
    }
}

