using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using westcoast_cars.api.ViewModels;
using Microsoft.Extensions.Logging;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/transmissionTypes")]
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
        public async Task<IActionResult> ListAll()
        {
            var transmissionTypes = await _unitOfWork.TransmissionTypes.ListAllAsync();
            var result = transmissionTypes.Select(t => new { Id = t.Id, Name = t.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transmissionType = await _unitOfWork.TransmissionTypes.FindByIdAsync(id);
            if (transmissionType is null) return NotFound();
            return Ok(transmissionType);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel model)
        {
            _logger.LogInformation("Received request to add a new transmission type.");
            _logger.LogInformation("Request payload: {@Model}", model);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Returning BadRequest.");
                return BadRequest("All information är inte con en el anropet");
            }

            var existing = await _unitOfWork.TransmissionTypes.ListAsync(t => t.Name.ToUpper() == model.Name.ToUpper());
            if (existing.Any())
            {
                _logger.LogWarning("Transmission type {Name} already exists.", model.Name);
                return BadRequest($"Transmission type {model.Name} finns redan en el systemet");
            }

            var modelToAdd = new TransmissionType
            {
                Name = model.Name
            };

            _unitOfWork.TransmissionTypes.Add(modelToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Transmission type {Name} created successfully.", model.Name);
                return CreatedAtAction(nameof(GetById), new { id = modelToAdd.Id }, new {
                    Id = modelToAdd.Id,
                    Name = modelToAdd.Name
                });
            }

            _logger.LogError("Failed to save transmission type {Name} to the database.", model.Name);
            return StatusCode(500, "Internal Server Error");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transmissionType = await _unitOfWork.TransmissionTypes.FindByIdAsync(id);
            if (transmissionType is null) return NotFound();

            _unitOfWork.TransmissionTypes.Delete(id);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return NoContent();
            }

            return StatusCode(500, "Internal Server Error");
        }
    }
}