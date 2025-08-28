using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using westcoast_cars.api.ViewModels;
using WestcoastCars.Infrastructure.Data;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/manufacturers")]
    public class ManufacturersController : ControllerBase
    {
        private readonly IManufacturerRepository _manufacturerRepository;
        private readonly WestcoastCarsContext _context; // Se mantiene para SaveChangesAsync

        public ManufacturersController(IManufacturerRepository manufacturerRepository, WestcoastCarsContext context)
        {
            _manufacturerRepository = manufacturerRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var manufacturers = await _manufacturerRepository.ListAllAsync();
            var result = manufacturers.Select(m => new { Name = m.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var manufacturer = await _manufacturerRepository.FindByIdAsync(id);
            if (manufacturer is null) return NotFound();
            return Ok(manufacturer);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var manufacturers = await _manufacturerRepository.ListAsync(c => c.Name.ToUpper().StartsWith(name.ToUpper()));
            return Ok(manufacturers);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehiclesByMake(string name)
        {
            var manufacturer = await _manufacturerRepository.FindByNameWithVehiclesAsync(name);
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

            var existing = await _manufacturerRepository.ListAsync(m => m.Name.ToUpper() == model.Name.ToUpper());
            if (existing.Any())
            {
                return BadRequest($"Manufacturer {model.Name} finns redan i systemet");
            }

            var modelToAdd = new Manufacturer
            {
                Name = model.Name
            };

            await _manufacturerRepository.AddAsync(modelToAdd);

            if (await _context.SaveChangesAsync() > 0)
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
