using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Api.Exceptions;
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
        [AllowAnonymous]
        public async Task<IActionResult> ListAll()
        {
            _logger.LogInformation("Retrieving list of unsold vehicles");
            
            var vehicles = await _unitOfWork.Vehicles.ListAsync(v => v.IsSold == false);
            
            var result = vehicles.Select(v => new VehicleSummaryDto
            {
                Id = v.Id,
                Name = $"{v.Manufacturer.Name} {v.Model}",
                Manufacturer = v.Manufacturer.Name,
                Model = v.Model,
                ModelYear = v.ModelYear,
                ImageUrl = v.ImageUrl ?? "/images/no-car.png"
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} vehicles", result.Count);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving vehicle with ID: {Id}", id);
            
            var v = await _unitOfWork.Vehicles.FindByIdAsync(id);

            if (v is null) 
            {
                _logger.LogWarning("Vehicle with ID {Id} not found", id);
                throw new NotFoundException($"Vehicle with ID {id} not found");
            }

            var result = new VehicleDetailsDto
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Name = $"{v.Manufacturer.Name} {v.Model}",
                Manufacturer = v.Manufacturer.Name,
                Model = v.Model,
                ModelYear = v.ModelYear,
                Mileage = v.Mileage,
                FuelType = v.FuelType.Name,
                TransmissionsType = v.TransmissionType.Name,
                Value = v.Value,
                Description = v.Description,
                ImageUrl = v.ImageUrl ?? "/images/no-car.png",
                IsSold = v.IsSold
            };

            _logger.LogInformation("Successfully retrieved vehicle {Id}", id);
            return Ok(result);
        }

        [HttpGet("regno/{regNo}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByRegNo(string regNo)
        {
            _logger.LogInformation("Retrieving vehicle with registration number: {RegNo}", regNo);
            
            var v = await _unitOfWork.Vehicles.FindByRegistrationNumberAsync(regNo);

            if (v is null) 
            {
                _logger.LogWarning("Vehicle with registration number {RegNo} not found", regNo);
                throw new NotFoundException($"Vehicle with registration number {regNo} not found");
            }
            
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

            _logger.LogInformation("Successfully retrieved vehicle by registration number {RegNo}", regNo);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Salesperson")]
        public async Task<IActionResult> Add(VehiclePostDto vehicle)
        {
            _logger.LogInformation("🚗 Creating new vehicle with registration: {RegNo}", vehicle.RegistrationNumber);
            _logger.LogInformation("Vehicle data: {@Vehicle}", vehicle);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid for vehicle creation: {Errors}", 
                    ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            // Check if vehicle already exists
            if (await _unitOfWork.Vehicles.FindByRegistrationNumberAsync(vehicle.RegistrationNumber) is not null)
            {
                _logger.LogWarning("Vehicle with registration {RegNo} already exists", vehicle.RegistrationNumber);
                throw new ConflictException($"Vehicle with registration number {vehicle.RegistrationNumber} already exists");
            }

            // Validate related entities
            var (manufacturer, fuelType, transmissionType) = await ValidateRelatedEntitiesAsync(vehicle.ManufacturerId, vehicle.FuelTypeId, vehicle.TransmissionTypeId);

            var vehicleToAdd = new Vehicle
            {
                RegistrationNumber = vehicle.RegistrationNumber,
                Manufacturer = manufacturer,
                Model = vehicle.Model,
                ModelYear = vehicle.ModelYear,
                Mileage = vehicle.Mileage,
                TransmissionType = transmissionType,
                FuelType = fuelType,
                Value = vehicle.Value,
                IsSold = vehicle.IsSold,
                Description = vehicle.Description,
                ImageUrl = string.IsNullOrEmpty(vehicle.ImageUrl) ? "no-car.png" : vehicle.ImageUrl
            };

            _unitOfWork.Vehicles.Add(vehicleToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("✅ Vehicle {RegNo} created successfully with ID {Id}", 
                    vehicle.RegistrationNumber, vehicleToAdd.Id);
                
                return CreatedAtAction(nameof(GetById), new { id = vehicleToAdd.Id }, new {
                    Id = vehicleToAdd.Id,
                    RegistrationNumber = vehicleToAdd.RegistrationNumber,
                    Model = vehicleToAdd.Model,
                    ModelYear = vehicleToAdd.ModelYear
                });
            }

            _logger.LogError("❌ Failed to save vehicle {RegNo} to database", vehicle.RegistrationNumber);
            return StatusCode(500, "Failed to create vehicle");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Salesperson")]
        public async Task<IActionResult> UpdateVehicle(int id, VehicleUpdateDto model)
        {
            _logger.LogInformation("🔄 Updating vehicle {Id}", id);
            _logger.LogInformation("Update data: {@Model}", model);

            if (!ModelState.IsValid) 
            {
                _logger.LogWarning("ModelState invalid for vehicle update: {Errors}", 
                    ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            var vehicle = await _unitOfWork.Vehicles.FindByIdAsync(id);
            if (vehicle is null) 
            {
                _logger.LogWarning("Vehicle {Id} not found for update", id);
                throw new NotFoundException($"Vehicle with ID {id} not found");
            }

            // 🔥 VALIDAR REGISTRATION NUMBER SI CAMBIÓ
            if (!string.IsNullOrEmpty(model.RegistrationNumber) && 
                model.RegistrationNumber != vehicle.RegistrationNumber)
            {
                var existingVehicle = await _unitOfWork.Vehicles.FindByRegistrationNumberAsync(model.RegistrationNumber);
                if (existingVehicle != null && existingVehicle.Id != id)
                {
                    _logger.LogWarning("Registration number {RegNo} already exists on another vehicle", model.RegistrationNumber);
                    throw new ConflictException($"Registration number {model.RegistrationNumber} already exists");
                }
            }

            // Validate related entities
            var (manufacturer, fuelType, transmissionType) = await ValidateRelatedEntitiesAsync(model.ManufacturerId, model.FuelTypeId, model.TransmissionTypeId);

            // 🔥 UPDATE ALL PROPERTIES INCLUDING REGISTRATION NUMBER
            if (!string.IsNullOrEmpty(model.RegistrationNumber))
                vehicle.RegistrationNumber = model.RegistrationNumber;
                
            vehicle.Model = model.Model;
            vehicle.ModelYear = model.ModelYear;
            vehicle.Manufacturer = manufacturer;
            vehicle.FuelType = fuelType;
            vehicle.TransmissionType = transmissionType;
            vehicle.Mileage = model.Mileage;
            vehicle.Description = model.Description;
            vehicle.Value = model.Value;
            vehicle.IsSold = model.IsSold;
            vehicle.ImageUrl = string.IsNullOrEmpty(model.ImageUrl) ? "no-car.png" : model.ImageUrl;

            _unitOfWork.Vehicles.Update(vehicle);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("✅ Vehicle {Id} updated successfully", id);
                return NoContent();
            }

            _logger.LogError("❌ Failed to update vehicle {Id}", id);
            return StatusCode(500, "Failed to update vehicle");
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Salesperson")]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            _logger.LogInformation("Marking vehicle {Id} as sold", id);
            
            var vehicle = await _unitOfWork.Vehicles.FindByIdAsync(id);
            if (vehicle is null) 
            {
                _logger.LogWarning("Vehicle {Id} not found for marking as sold", id);
                throw new NotFoundException($"Vehicle with ID {id} not found");
            }

            vehicle.IsSold = true;
            _unitOfWork.Vehicles.Update(vehicle);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("✅ Vehicle {Id} marked as sold successfully", id);
                return NoContent();
            }

            _logger.LogError("❌ Failed to mark vehicle {Id} as sold", id);
            return StatusCode(500, "Failed to mark vehicle as sold");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting vehicle {Id}", id);
            
            var vehicle = await _unitOfWork.Vehicles.FindByIdAsync(id);
            if (vehicle is null) 
            {
                _logger.LogWarning("Vehicle {Id} not found for deletion", id);
                throw new NotFoundException($"Vehicle with ID {id} not found");
            }

            _unitOfWork.Vehicles.Delete(id);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                _logger.LogInformation("✅ Vehicle {Id} deleted successfully", id);
                return NoContent();
            }

            _logger.LogError("❌ Failed to delete vehicle {Id}", id);
            return StatusCode(500, "Failed to delete vehicle");
        }

        // 🔧 MÉTODO HELPER PARA VALIDAR ENTIDADES RELACIONADAS
        private async Task<(Manufacturer, FuelType, TransmissionType)> ValidateRelatedEntitiesAsync(int manufacturerId, int fuelTypeId, int transmissionTypeId)
        {
            var manufacturer = await _unitOfWork.Manufacturers.FindByIdAsync(manufacturerId);
            if (manufacturer is null)
            {
                throw new NotFoundException($"Manufacturer with ID {manufacturerId} not found");
            }

            var fuelType = await _unitOfWork.FuelTypes.FindByIdAsync(fuelTypeId);
            if (fuelType is null)
            {
                throw new NotFoundException($"Fuel type with ID {fuelTypeId} not found");
            }

            var transmissionType = await _unitOfWork.TransmissionTypes.FindByIdAsync(transmissionTypeId);
            if (transmissionType is null)
            {
                throw new NotFoundException($"Transmission type with ID {transmissionTypeId} not found");
            }

            return (manufacturer, fuelType, transmissionType);
        }
    }
}