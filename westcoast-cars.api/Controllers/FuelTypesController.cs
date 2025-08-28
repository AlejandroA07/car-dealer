using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Data; // Keep for saving

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/fueltypes")]
    public class FuelTypesController : ControllerBase
    {
        private readonly IFuelTypeRepository _fuelTypeRepository;
        private readonly WestcoastCarsContext _context; // Keep for SaveChangesAsync

        public FuelTypesController(IFuelTypeRepository fuelTypeRepository, WestcoastCarsContext context)
        {
            _fuelTypeRepository = fuelTypeRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var fuelTypes = await _fuelTypeRepository.ListAllAsync();
            var result = fuelTypes.Select(c => new { Name = c.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")] // Corrected route
        public async Task<IActionResult> GetById(int id)
        {
            var fuelType = await _fuelTypeRepository.FindByIdAsync(id);
            if (fuelType is null) return NotFound();
            
            // Corrected implementation
            var result = new { Name = fuelType.Name };
            return Ok(result);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehicles(string name)
        {
            var fuelType = await _fuelTypeRepository.FindByNameWithVehiclesAsync(name);
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
    }
}
