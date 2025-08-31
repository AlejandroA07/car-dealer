using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using westcoast_cars.api.ViewModels;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/transmissionTypes")]
    public class TransmissionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransmissionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> ListAll()
        {
            var transmissions = await _unitOfWork.TransmissionTypes.ListAllAsync();
            var result = transmissions.Select(c => new { c.Id, c.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transmission = await _unitOfWork.TransmissionTypes.FindByIdAsync(id);
            if (transmission is null) return NotFound();
            
            return Ok(new { transmission.Id, transmission.Name });
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var transmissions = await _unitOfWork.TransmissionTypes.ListAsync(t => t.Name.ToUpper().Contains(name.ToUpper()));
            var result = transmissions.Select(t => new { t.Id, t.Name }).ToList();
            return Ok(result);
        }

        [HttpGet("{name}/vehicles")]
        public async Task<IActionResult> ListVehiclesByTransmissionType(string name)
        {
            var transmissions = await _unitOfWork.TransmissionTypes.FindByNameWithVehiclesAsync(name);
            if (transmissions is null || !transmissions.Any()) return NotFound();

            var result = transmissions.Select(t => new
            {
                Name = t.Name,
                Vehicles = t.Vehicles.Select(v => new
                {
                    RegistrationNumber = v.RegistrationNumber,
                    Model = v.Model,
                    Manufacturer = v.Manufacturer.Name,
                    ModelYear = v.ModelYear
                }).ToList()
            }).ToList();
            
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest("All information is not included in the call.");

            var existing = await _unitOfWork.TransmissionTypes.ListAsync(m => m.Name.ToUpper() == model.Name.ToUpper());
            if (existing.Any())
            {
                return BadRequest($"Transmission type {model.Name} already exists.");
            }

            var modelToAdd = new TransmissionType
            {
                Name = model.Name
            };

            _unitOfWork.TransmissionTypes.Add(modelToAdd);

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
