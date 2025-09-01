using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using westcoast_cars.api.ViewModels;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/manufacturers")]
    public class ManufacturersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ManufacturersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var manufacturers = await _unitOfWork.Manufacturers.ListAllAsync();
            var result = manufacturers.Select(m => new { Id = m.Id, Name = m.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var manufacturer = await _unitOfWork.Manufacturers.FindByIdAsync(id);
            if (manufacturer is null) return NotFound();
            return Ok(manufacturer);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var manufacturers = await _unitOfWork.Manufacturers.ListAsync(c => c.Name.ToUpper().StartsWith(name.ToUpper()));
            return Ok(manufacturers);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehiclesByMake(string name)
        {
            var manufacturer = await _unitOfWork.Manufacturers.FindByNameWithVehiclesAsync(name);
            if (manufacturer is null) return NotFound();

            var result = new
            {
                Name = manufacturer.Name,
                Vehicles = manufacturer.Vehicles.Select(v => new
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
            if (!ModelState.IsValid) return BadRequest("All information är inte med i anropet");

            var existing = await _unitOfWork.Manufacturers.ListAsync(m => m.Name.ToUpper() == model.Name.ToUpper());
            if (existing.Any())
            {
                return BadRequest($"Manufacturer {model.Name} finns redan i systemet");
            }

            var modelToAdd = new Manufacturer
            {
                Name = model.Name
            };

            _unitOfWork.Manufacturers.Add(modelToAdd);

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
