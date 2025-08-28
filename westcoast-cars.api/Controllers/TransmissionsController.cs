using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/transmissionTypes")]
    public class TransmissionsController : ControllerBase
    {
        private readonly ITransmissionTypeRepository _transmissionRepository;

        public TransmissionsController(ITransmissionTypeRepository transmissionRepository)
        {
            _transmissionRepository = transmissionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var transmissions = await _transmissionRepository.ListAllAsync();
            var result = transmissions.Select(c => new { Name = c.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transmission = await _transmissionRepository.FindByIdWithVehiclesAsync(id);
            if (transmission is null) return NotFound();

            var result = new
            {
                TransmissionTypeName = transmission.Name,
                Vehicles = transmission.Vehicles.Select(m => new
                {
                    VehicleName = $"{m.Manufacturer.Name} {m.Model}",
                    TransmissionType = m.TransmissionsType.Name
                }).ToList()
            };
            return Ok(result);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var transmissions = await _transmissionRepository.FindByNameWithVehiclesAsync(name);
            var result = transmissions.Select(t => new
            {
                Name = t.Name,
                Vehicles = t.Vehicles.Select(v => new
                {
                    RegistraitionNumber = v.RegistrationNumber,
                    Model = v.Model,
                    Manufacturer = v.Manufacturer.Name,
                    ModelYear = v.ModelYear
                })
            }).ToList();
            
            return Ok(result);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehiclesByTransmissionTypes(string name)
        {
            var transmissions = await _transmissionRepository.FindByNameWithVehiclesAsync(name);
            var result = transmissions.Select(t => new
            {
                Name = t.Name,
                Vehicles = t.Vehicles.Select(m => new
                {
                    Name = $"{m.Manufacturer.Name} {m.Model}",
                    RegistrationNumber = m.RegistrationNumber,
                    Model = m.Model
                }).ToList()
            }).ToList();

            return Ok(result);
        }
    }
}
