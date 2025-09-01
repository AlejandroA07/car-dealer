using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using westcoast_cars.api.ViewModels;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/fueltypes")]
    public class FuelTypesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public FuelTypesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var fuelTypes = await _unitOfWork.FuelTypes.ListAllAsync();
            var result = fuelTypes.Select(c => new { Id = c.Id, Name = c.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var fuelType = await _unitOfWork.FuelTypes.FindByIdAsync(id);
            if (fuelType is null) return NotFound();
            
            var result = new { Name = fuelType.Name };
            return Ok(result);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehicles(string name)
        {
            var fuelType = await _unitOfWork.FuelTypes.FindByNameWithVehiclesAsync(name);
            if (fuelType is null) return NotFound();

            var result = new
            {
                Name = fuelType.Name,
                Vehicles = fuelType.Vehicles.Select(v => new
                {
                    RegistrationNumber = v.RegistrationNumber,
                    Model = v.Model,
                    ModelYear = v.ModelYear,
                    Mileage = v.Mileage
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest("All information is not included in the call");

            var existing = await _unitOfWork.FuelTypes.ListAsync(m => m.Name.ToUpper() == model.Name.ToUpper());
            if (existing.Any())
            {
                return BadRequest($"Fuel type {model.Name} already exists.");
            }

            var modelToAdd = new FuelType
            {
                Name = model.Name
            };

            _unitOfWork.FuelTypes.Add(modelToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = modelToAdd.Id }, new {
                    Id = modelToAdd.Id,
                    Name = modelToAdd.Name
                });
            }

            return StatusCode(500, "Internal Server Error");
        }
    }
}
