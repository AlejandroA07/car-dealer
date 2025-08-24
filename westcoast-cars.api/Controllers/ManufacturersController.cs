using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using westcoast_cars.api.Data;
using westcoast_cars.api.Entities;
using westcoast_cars.api.ViewModels;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/manufacturers")]
    public class ManufacturersController : ControllerBase
    {
        private readonly WestcoastCarsContext _context;
        public ManufacturersController(WestcoastCarsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var result = await _context.Manufacturers
                .Select(v => new 
                {
                    Name = v.Name
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _context.Manufacturers.FindAsync(id);
            return Ok(result);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _context.Manufacturers
                .Where(c => c.Name.ToUpper().StartsWith(name.ToUpper()))
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehiclesByMake(string name)
        {
            var result = await _context.Manufacturers
                .Where(c => c.Name.ToUpper().StartsWith(name.ToUpper()))
                .Select(m => new
                {
                    Name = m.Name,
                    Vehicles = m.Vehicles.Select(v => new
                    {
                        RegistrationNumber = v.RegistrationNumber,
                        Model = v.Model,
                        ModelYear = v.ModelYear,
                        Mileage = v.Mileage
                    }).ToList()
                }).ToListAsync();


            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest("All information är inte med i anropet");

            if(await _context.Manufacturers.SingleOrDefaultAsync(c => c.Name.ToUpper() == model.Name.ToUpper()) is not null)
            {
                return BadRequest($"Manufacturer {model.Name} finns redan i systemet");
            }


            var modelToAdd = new Manufacturer
            {
                Name = model.Name
            };

            try
            {
                await _context.Manufacturers.AddAsync(modelToAdd);

                if(await _context.SaveChangesAsync() > 0)
                {
                    return CreatedAtAction(nameof(GetById), new { id = modelToAdd.Id},
                    new
                    {
                        Id = modelToAdd.Id,
                        Name = modelToAdd.Name
                    });
                }

                return StatusCode(500, "Internal Server Error");
            }
            catch (Exception ex)
            {
                // loggning till en databas som hanterar debug information...
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        
    }
}