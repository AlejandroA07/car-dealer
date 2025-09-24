
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
        [AllowAnonymous]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Retrieving all manufacturers");
            var manufacturers = await _unitOfWork.Repository<Manufacturer>().GetAllAsync();
            var result = manufacturers.Select(m => new NamedObjectDto { Id = m.Id, Name = m.Name }).ToList();
            _logger.LogInformation("Successfully retrieved {Count} manufacturers", result.Count);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving manufacturer with ID: {Id}", id);
            var manufacturer = await _unitOfWork.Repository<Manufacturer>().GetByIdAsync(id);
            if (manufacturer is null)
            {
                _logger.LogWarning("Manufacturer with ID {Id} not found", id);
                throw new NotFoundException($"Manufacturer with ID {id} not found");
            }
            _logger.LogInformation("Successfully retrieved manufacturer with ID {Id}", id);
            return Ok(manufacturer);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            _logger.LogInformation("Attempting to add new manufacturer: {Name}", model.Name);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for new manufacturer.");
                return BadRequest(ModelState);
            }

            var existing = await _unitOfWork.Repository<Manufacturer>().FirstOrDefaultAsync(m => m.Name.ToUpper() == model.Name.ToUpper());
            if (existing != null)
            {
                _logger.LogWarning("Manufacturer '{Name}' already exists.", model.Name);
                throw new ConflictException($"Manufacturer '{model.Name}' already exists.");
            }

            var manufacturerToAdd = new Manufacturer { Name = model.Name };
            await _unitOfWork.Repository<Manufacturer>().AddAsync(manufacturerToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Successfully added new manufacturer with ID {Id}", manufacturerToAdd.Id);
                return CreatedAtAction(nameof(GetById), new { id = manufacturerToAdd.Id }, manufacturerToAdd);
            }

            _logger.LogError("Failed to add manufacturer '{Name}' to the database.", model.Name);
            return StatusCode(500, "Failed to save manufacturer.");
        }

        

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Attempting to delete manufacturer with ID: {Id}", id);
            var manufacturerToDelete = await _unitOfWork.Repository<Manufacturer>().GetByIdAsync(id);
            if (manufacturerToDelete is null)
            {
                _logger.LogWarning("Manufacturer with ID {Id} not found for deletion.", id);
                throw new NotFoundException($"Manufacturer with ID {id} not found.");
            }

            _unitOfWork.Repository<Manufacturer>().Remove(manufacturerToDelete);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Successfully deleted manufacturer with ID {Id}", id);
                return NoContent();
            }

            _logger.LogError("Failed to delete manufacturer with ID {Id}", id);
            return StatusCode(500, "Failed to delete manufacturer.");
        }
    }
}