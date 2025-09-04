using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using westcoast_cars.api.ViewModels;
using Microsoft.Extensions.Logging;

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
            var fuelTypes = await _unitOfWork.FuelTypes.ListAllAsync();
            var result = fuelTypes.Select(f => new { Id = f.Id, Name = f.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var fuelType = await _unitOfWork.FuelTypes.FindByIdAsync(id);
            if (fuelType is null) return NotFound();
            return Ok(fuelType);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel model)
        {
            _logger.LogInformation("Received request to add a new fuel type.");
            _logger.LogInformation("Request payload: {@Model}", model);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Returning BadRequest.");
                return BadRequest("All information är inte med i anropet");
            }

            var existing = await _unitOfWork.FuelTypes.ListAsync(f => f.Name.ToUpper() == model.Name.ToUpper());
            if (existing.Any())
            {
                _logger.LogWarning("Fuel type {Name} already exists.", model.Name);
                return BadRequest($"Fuel type {model.Name} finns redan i systemet");
            }

            var modelToAdd = new FuelType
            {
                Name = model.Name
            };

            _unitOfWork.FuelTypes.Add(modelToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Fuel type {Name} created successfully.", model.Name);
                return CreatedAtAction(nameof(GetById), new { id = modelToAdd.Id }, new {
                    Id = modelToAdd.Id,
                    Name = modelToAdd.Name
                });
            }

            _logger.LogError("Failed to save fuel type {Name} to the database.", model.Name);
            return StatusCode(500, "Internal Server Error");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var fuelType = await _unitOfWork.FuelTypes.FindByIdAsync(id);
            if (fuelType is null) return NotFound();

            _unitOfWork.FuelTypes.Delete(id);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return NoContent();
            }

            return StatusCode(500, "Internal Server Error");
        }
    }
}