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
    [Route("api/v1/transmissions")]
    public class TransmissionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransmissionsController> _logger;

        public TransmissionsController(IUnitOfWork unitOfWork, ILogger<TransmissionsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Retrieving all transmission types");
            var transmissionTypes = await _unitOfWork.TransmissionTypes.ListAllAsync();
            var result = transmissionTypes.Select(t => new { Id = t.Id, Name = t.Name }).ToList();
            _logger.LogInformation("Successfully retrieved {Count} transmission types", result.Count);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving transmission type with ID: {Id}", id);
            var transmissionType = await _unitOfWork.TransmissionTypes.FindByIdAsync(id);
            if (transmissionType is null)
            {
                _logger.LogWarning("Transmission type with ID {Id} not found", id);
                throw new NotFoundException($"Transmission type with ID {id} not found");
            }
            _logger.LogInformation("Successfully retrieved transmission type with ID {Id}", id);
            return Ok(transmissionType);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            _logger.LogInformation("Attempting to add new transmission type: {Name}", model.Name);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for new transmission type.");
                return BadRequest(ModelState);
            }

            var existing = await _unitOfWork.TransmissionTypes.ListAsync(t => t.Name.ToUpper() == model.Name.ToUpper());
            if (existing.Any())
            {
                _logger.LogWarning("Transmission type '{Name}' already exists.", model.Name);
                throw new ConflictException($"Transmission type '{model.Name}' already exists.");
            }

            var transmissionTypeToAdd = new TransmissionType { Name = model.Name };
            _unitOfWork.TransmissionTypes.Add(transmissionTypeToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Successfully added new transmission type with ID {Id}", transmissionTypeToAdd.Id);
                return CreatedAtAction(nameof(GetById), new { id = transmissionTypeToAdd.Id }, transmissionTypeToAdd);
            }

            _logger.LogError("Failed to add transmission type '{Name}' to the database.", model.Name);
            return StatusCode(500, "Failed to save transmission type.");
        }

        

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Attempting to delete transmission type with ID: {Id}", id);
            var transmissionTypeToDelete = await _unitOfWork.TransmissionTypes.FindByIdAsync(id);
            if (transmissionTypeToDelete is null)
            {
                _logger.LogWarning("Transmission type with ID {Id} not found for deletion.", id);
                throw new NotFoundException($"Transmission type with ID {id} not found.");
            }

            _unitOfWork.TransmissionTypes.Delete(id);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Successfully deleted transmission type with ID {Id}", id);
                return NoContent();
            }

            _logger.LogError("Failed to delete transmission type with ID {Id}", id);
            return StatusCode(500, "Failed to delete transmission type.");
        }
    }
}
