
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

namespace WestcoastCars.Api.Controllers
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
            var transmissionTypes = await _unitOfWork.Repository<TransmissionType>().GetAllAsync();
            var result = transmissionTypes.Select(t => new NamedObjectDto { Id = t.Id, Name = t.Name }).ToList();
            _logger.LogInformation("Successfully retrieved {Count} transmission types", result.Count);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving transmission type with ID: {Id}", id);
            var transmissionType = await _unitOfWork.Repository<TransmissionType>().GetByIdAsync(id);
            if (transmissionType is null)
            {
                _logger.LogWarning("Transmission type with ID {Id} not found", id);
                throw new NotFoundException($"Transmission type with ID {id} not found");
            }
            _logger.LogInformation("Successfully retrieved transmission type with ID {Id}", id);
            var result = new NamedObjectDto { Id = transmissionType.Id, Name = transmissionType.Name };
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] NamedObjectDto model)
        {
            _logger.LogDebug("Attempting to add new transmission type: {Name}", model?.Name);
            if (model is null)
            {
                _logger.LogInformation("Request body is null.");
                return BadRequest("Request body cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid model state for new transmission type.");
                return BadRequest(ModelState);
            }

            var existing = await _unitOfWork.Repository<TransmissionType>().FirstOrDefaultAsync(t => t.Name.ToUpper() == model.Name.ToUpper());
            if (existing != null)
            {
                _logger.LogInformation("Transmission type '{Name}' already exists.", model.Name);
                throw new ConflictException($"Transmission type '{model.Name}' already exists.");
            }

            var transmissionTypeToAdd = new TransmissionType { Name = model.Name };
            await _unitOfWork.Repository<TransmissionType>().AddAsync(transmissionTypeToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Successfully added new transmission type with ID {Id}", transmissionTypeToAdd.Id);
                return CreatedAtAction(nameof(GetById), new { id = transmissionTypeToAdd.Id }, new NamedObjectDto { Id = transmissionTypeToAdd.Id, Name = transmissionTypeToAdd.Name });
            }

            _logger.LogError("Failed to add transmission type '{Name}' to the database.", model.Name);
            return StatusCode(500, "Failed to save transmission type.");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] NamedObjectDto model)
        {
            _logger.LogDebug("Attempting to update transmission type with ID: {Id}", id);
            if (model is null)
            {
                _logger.LogInformation("Request body is null.");
                return BadRequest("Request body cannot be null.");
            }

            if (id != model.Id)
            {
                _logger.LogInformation("Mismatched ID in request body for transmission type update.");
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid model state for transmission type update.");
                return BadRequest(ModelState);
            }

            var transmissionTypeToUpdate = await _unitOfWork.Repository<TransmissionType>().GetByIdAsync(id);
            if (transmissionTypeToUpdate is null)
            {
                _logger.LogInformation("Transmission type with ID {Id} not found for update.", id);
                throw new NotFoundException($"Transmission type with ID {id} not found.");
            }

            var existing = await _unitOfWork.Repository<TransmissionType>().FirstOrDefaultAsync(t => t.Name.ToUpper() == model.Name.ToUpper() && t.Id != id);
            if (existing != null)
            {
                _logger.LogInformation("Transmission type name '{Name}' already exists.", model.Name);
                throw new ConflictException($"A transmission type with the name '{model.Name}' already exists.");
            }

            transmissionTypeToUpdate.Name = model.Name;
            _unitOfWork.Repository<TransmissionType>().Update(transmissionTypeToUpdate);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Successfully updated transmission type with ID {Id}", id);
                return NoContent();
            }

            _logger.LogError("Failed to update transmission type with ID {Id}", id);
            return StatusCode(500, "Failed to update transmission type.");
        }


        

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogDebug("Attempting to delete transmission type with ID: {Id}", id);
            var transmissionTypeToDelete = await _unitOfWork.Repository<TransmissionType>().GetByIdAsync(id);
            if (transmissionTypeToDelete is null)
            {
                _logger.LogInformation("Transmission type with ID {Id} not found for deletion.", id);
                throw new NotFoundException($"Transmission type with ID {id} not found.");
            }

            _unitOfWork.Repository<TransmissionType>().Remove(transmissionTypeToDelete);

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

