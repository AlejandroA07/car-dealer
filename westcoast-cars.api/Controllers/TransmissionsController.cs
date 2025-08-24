using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using westcoast_cars.api.Data;
using westcoast_cars.api.Entities;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/transmissionTypes")]
    public class TransmissionsController : ControllerBase
    {
        private readonly WestcoastCarsContext _context;
        public TransmissionsController(WestcoastCarsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var result = await _context.TransmissionTypes
                .Select(c => new
                {
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _context.TransmissionTypes
                .Include(c => c.Vehicles)
                .Select(v => new
                {
                    TransmissionTypeName = v.Name,
                    Vehicles = v.Vehicles.Select(m => new 
                    {
                        VehicleName = m.Name,
                        TransmissionType = m.TransmissionsType.Name
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return Ok(result);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _context.TransmissionTypes
                .Where(c => c.Name.ToUpper().StartsWith(name.ToUpper()))
                .Select(c => new
                {
                    Name = c.Name,
                    Vehicles = c.Vehicles.Select(v => new
                    {
                        RegistraitionNumber = v.RegistrationNumber,
                        Model = v.Model,
                        Manufacturer = v.Manufacturer.Name,
                        ModelYear = v.ModelYear
                    })
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehiclesByTransmissionTypes(string name)
        {
            var result = await _context.TransmissionTypes
                .Where(c => c.Name.ToUpper().StartsWith(name.ToUpper()))
                .Select(v => new 
                {
                    Name = v.Name,
                    Vehicles = v.Vehicles.Select(m => new 
                    {
                        Name = m.Name,
                        RegistrationNumber = m.RegistrationNumber,
                        Model = m.Model
                    
                    }).ToList()

                })
                .ToListAsync();

            return Ok(result);
        }

    }
}