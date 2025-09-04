using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using westcoast_cars.api.ViewModels;
using Microsoft.Extensions.Logging;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _imageBaseUrl;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IUnitOfWork unitOfWork, IConfiguration config, ILogger<VehiclesController> logger)
        {
            _unitOfWork = unitOfWork;
            _imageBaseUrl = config.GetSection("apiImageUrl").Value;
            _logger = logger;
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListAll()
        {
            var vehicles = await _unitOfWork.Vehicles.ListAsync(v => v.IsSold == false);
            
            var result = vehicles.Select(v => new {
                Id = v.Id,
                Name = $"{v.Manufacturer.Name} {v.Model}",
                Manufacturer = v.Manufacturer.Name,
                Model = v.Model,
                ModelYear = v.ModelYear,
                Mileage = v.Mileage,
                                ImageUrl = v.ImageUrl ?? "/images/no-car.png"
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var v = await _unitOfWork.Vehicles.FindByIdAsync(id);

            if (v is null) return NotFound();

            var result = new {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Name = $"{v.Manufacturer.Name} {v.Model}",
                Make = v.Manufacturer.Name,
                Model = v.Model,
                Modelyear = v.ModelYear,
                Milage = v.Mileage,
                FuelType = v.FuelType.Name,
                Transmission = v.TransmissionType.Name,
                Value = v.Value,
                Description = v.Description,
                                ImageUrl = v.ImageUrl ?? "/images/no-car.png"
            };

            return Ok(result);
        }

        [HttpGet("regno/{regNo}")]
        public async Task<IActionResult> GetByRegNo(string regNo)
        {
            var v = await _unitOfWork.Vehicles.FindByRegistrationNumberAsync(regNo);

            if (v is null) return NotFound();
            
            var result = new {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Name = $"{v.Manufacturer.Name} {v.Model}",
                Modelyear = v.ModelYear,
                Milage = v.Mileage,
                FuelType = v.FuelType.Name,
                Transmission = v.TransmissionType.Name,
                ImageUrl = v.ImageUrl    
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(VehiclePostViewModel vehicle)
        {
            _logger.LogInformation("Received request to add a new vehicle.");
            _logger.LogInformation("Request payload: {@Vehicle}", vehicle);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Returning BadRequest.");
                return BadRequest("All information är inte med i anropet");
            }

            if (await _unitOfWork.Vehicles.FindByRegistrationNumberAsync(vehicle.RegistrationNumber) is not null)
            {
                _logger.LogWarning("Vehicle with registration number {RegNo} already exists.", vehicle.RegistrationNumber);
                return BadRequest($"Bilen med regnummer {vehicle.RegistrationNumber} finns redan i systemet");
            }

            var make = await _unitOfWork.Manufacturers.FindByIdAsync(vehicle.ManufacturerId);
            if (make is null)
            {
                _logger.LogWarning("Manufacturer with Id {MakeId} not found.", vehicle.ManufacturerId);
                return NotFound($"Tillverkare med Id {vehicle.ManufacturerId} hittades inte.");
            }

            var fueltype = await _unitOfWork.FuelTypes.FindByIdAsync(vehicle.FuelTypeId);
            if (fueltype is null)
            {
                _logger.LogWarning("FuelType with Id {FuelTypeId} not found.", vehicle.FuelTypeId);
                return NotFound($"Bränsletyp med Id {vehicle.FuelTypeId} hittades inte.");
            }

            var transmission = await _unitOfWork.TransmissionTypes.FindByIdAsync(vehicle.TransmissionTypeId);
            if (transmission is null)
            {
                _logger.LogWarning("TransmissionType with Id {TransmissionTypeId} not found.", vehicle.TransmissionTypeId);
                return NotFound($"Växellådstyp med Id {vehicle.TransmissionTypeId} hittades inte.");
            }

            var vehicleToAdd = new Vehicle
            {
                RegistrationNumber = vehicle.RegistrationNumber,
                Manufacturer = make,
                Model = vehicle.Model,
                ModelYear = vehicle.ModelYear,
                Mileage = vehicle.Mileage,
                TransmissionType = transmission,
                FuelType = fueltype,
                Value = vehicle.Value,
                IsSold = vehicle.IsSold,
                Description = vehicle.Description,
                ImageUrl = "no-car.png"
            };

            _unitOfWork.Vehicles.Add(vehicleToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("Vehicle with registration number {RegNo} created successfully.", vehicle.RegistrationNumber);
                return CreatedAtAction(nameof(GetById), new { id = vehicleToAdd.Id }, new {
                    Id = vehicleToAdd.Id,
                    RegistrationNumber = vehicleToAdd.RegistrationNumber,
                    Model = vehicleToAdd.Model,
                    ModelYear = vehicleToAdd.ModelYear
                });
            }

            _logger.LogError("Failed to save vehicle with registration number {RegNo} to the database.", vehicle.RegistrationNumber);
            return StatusCode(500, "Internal Server Error");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, VehicleUpdateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest("Felaktig information");

            var vehicle = await _unitOfWork.Vehicles.FindByIdAsync(id);
            if (vehicle is null) return BadRequest("Tyvärr vi kan inte hitta bilen som ska ändras");

            // Efficiently find related entities by their primary key (ID)
            var make = await _unitOfWork.Manufacturers.FindByIdAsync(model.ManufacturerId);
            if (make is null) return NotFound($"Tillverkare med Id {model.ManufacturerId} hittades inte.");

            var fueltype = await _unitOfWork.FuelTypes.FindByIdAsync(model.FuelTypeId);
            if (fueltype is null) return NotFound($"Bränsletyp med Id {model.FuelTypeId} hittades inte.");

            var transmission = await _unitOfWork.TransmissionTypes.FindByIdAsync(model.TransmissionTypeId);
            if (transmission is null) return NotFound($"Växellådstyp med Id {model.TransmissionTypeId} hittades inte.");

            // Update vehicle properties
            vehicle.Model = model.Model;
            vehicle.ModelYear = model.ModelYear;
            vehicle.Manufacturer = make;
            vehicle.FuelType = fueltype;
            vehicle.TransmissionType = transmission;
            vehicle.Mileage = model.Mileage;
            vehicle.Description = model.Description;
            vehicle.Value = model.Value;
            vehicle.IsSold = model.IsSold;
            vehicle.ImageUrl = string.IsNullOrEmpty(model.ImageUrl) ? "no-car.png" : model.ImageUrl;

            _unitOfWork.Vehicles.Update(vehicle);

            if (await _unitOfWork.CompleteAsync() > 0)
                return NoContent();

            return StatusCode(500, "Internal Server Error");
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            var vehicle = await _unitOfWork.Vehicles.FindByIdAsync(id);
            if (vehicle is null) return NotFound("Hittade inte bilen");

            vehicle.IsSold = true;

            _unitOfWork.Vehicles.Update(vehicle);

            if (await _unitOfWork.CompleteAsync() > 0)
                return NoContent();

            return StatusCode(500, "Internal Server Error");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _unitOfWork.Vehicles.FindByIdAsync(id);
            if (vehicle is null) return NotFound();

            _unitOfWork.Vehicles.Delete(id);

            if (await _unitOfWork.CompleteAsync() > 0)
                return NoContent();

            return StatusCode(500, "Internal Server Error");
        }
    }
}