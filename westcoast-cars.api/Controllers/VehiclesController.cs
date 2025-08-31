using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using westcoast_cars.api.ViewModels;

namespace westcoast_cars.api.Controllers
{
    [ApiController]
    [Route("api/v1/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _imageBaseUrl;

        public VehiclesController(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _imageBaseUrl = config.GetSection("apiImageUrl").Value;
        }

        [HttpGet]
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
                ImageUrl = _imageBaseUrl + v.ImageUrl ?? "no-car.png"
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
                Transmission = v.TransmissionsType.Name,
                Value = v.Value,
                Description = v.Description,
                ImageUrl = _imageBaseUrl + v.ImageUrl ?? "no-car.png"
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
                Transmission = v.TransmissionsType.Name,
                ImageUrl = v.ImageUrl    
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(VehiclePostViewModel vehicle)
        {
            if (!ModelState.IsValid) return BadRequest("All information är inte med i anropet");

            if (await _unitOfWork.Vehicles.FindByRegistrationNumberAsync(vehicle.RegistrationNumber) is not null)
            {
                return BadRequest($"Bilen med regnummer {vehicle.RegistrationNumber} finns redan i systemet");
            }

            var make = (await _unitOfWork.Manufacturers.ListAsync(m => m.Name.ToUpper() == vehicle.Make.ToUpper())).FirstOrDefault();
            if (make is null) return NotFound($"Tyvärr vi kunde inte hitta en tillverkare med namnet {vehicle.Make}");

            var fueltype = (await _unitOfWork.FuelTypes.ListAsync(f => f.Name.ToUpper() == vehicle.FuelType.ToUpper())).FirstOrDefault();
            if (fueltype is null) return NotFound($"Tyvärr vi kunde inte hitta en bränsletyp med namnet {vehicle.FuelType}");

            var transmission = (await _unitOfWork.TransmissionTypes.ListAsync(t => t.Name.ToUpper() == vehicle.Transmission.ToUpper())).FirstOrDefault();
            if (transmission is null) return NotFound($"Tyvärr vi kunde inte hitta en tillverkare med namnet {vehicle.Transmission}");

            var vehicleToAdd = new Vehicle
            {
                RegistrationNumber = vehicle.RegistrationNumber,
                Manufacturer = make,
                Model = vehicle.Model,
                ModelYear = vehicle.ModelYear,
                Mileage = vehicle.Mileage,
                TransmissionsType = transmission,
                FuelType = fueltype,
                Value = vehicle.Value,
                IsSold = vehicle.IsSold,
                Description = vehicle.Description,
                ImageUrl = "no-car.png"
            };

            _unitOfWork.Vehicles.Add(vehicleToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = vehicleToAdd.Id }, new {
                    Id = vehicleToAdd.Id,
                    RegistrationNumber = vehicleToAdd.RegistrationNumber,
                    Model = vehicleToAdd.Model,
                    ModelYear = vehicleToAdd.ModelYear
                });
            }

            return StatusCode(500, "Internal Server Error");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, VehicleUpdateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest("Felaktig information");

            var vehicle = await _unitOfWork.Vehicles.FindByIdAsync(id);
            if (vehicle is null) return BadRequest("Tyvärr vi kan inte hitta bilen som ska ändras");

            var make = (await _unitOfWork.Manufacturers.ListAsync(m => m.Name.ToUpper() == model.Make.ToUpper())).FirstOrDefault();
            if (make is null) return NotFound($"Tyvärr vi kunde inte hitta en tillverkare med namnet {model.Make}");

            var fueltype = (await _unitOfWork.FuelTypes.ListAsync(f => f.Name.ToUpper() == model.FuelType.ToUpper())).FirstOrDefault();
            if (fueltype is null) return NotFound($"Tyvärr vi kunde inte hitta en bränsletyp med namnet {model.FuelType}");

            var transmission = (await _unitOfWork.TransmissionTypes.ListAsync(t => t.Name.ToUpper() == model.Transmission.ToUpper())).FirstOrDefault();
            if (transmission is null) return NotFound($"Tyvärr vi kunde inte hitta en tillverkare med namnet {model.Transmission}");

            vehicle.Model = model.Model;
            vehicle.ModelYear = model.ModelYear;
            vehicle.Manufacturer = make;
            vehicle.FuelType = fueltype;
            vehicle.TransmissionsType = transmission;
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