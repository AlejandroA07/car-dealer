using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Infrastructure.Data;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/fueltypes")]
    public class FuelTypesController : ControllerBase
    {
        private readonly WestcoastCarsContext _context;
        public FuelTypesController(WestcoastCarsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var result = await _context.FuelTypes
            .Select(c => new 
            {
                Name = c.Name
            })
            .ToListAsync();

            return Ok(result);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _context.TransmissionTypes
                .Select(c => new 
                {
                    Name = c.Name
                })
                .ToListAsync();
            
            return Ok(result);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehicles(string name)
        {
            var result = await _context.FuelTypes
                .Where(c => c.Name.ToUpper().StartsWith(name.ToUpper()))
                .Select(f => new
                {
                    Name = f.Name,
                    Vehicles = f.Vehicles.Select(v => new
                    {
                        RegistrationNumber = v.RegistrationNumber,
                        Model = v.Model,
                        ModelYear = v.ModelYear,
                        Mileage = v.Mileage
                    }).ToList()
                })
                .ToListAsync();

            return Ok(result);
        }

    }
}