
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
        private const string DefaultCarImageName = "no-car.png";
        private const string DefaultCarImagePath = "/images/no-car.png";

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
            
            var vehicles = await _unitOfWork.VehicleRepository.GetAllAsync();
            
            var result = vehicles.Select(v => new VehicleSummaryDto
            {
                Id = v.Id,
                Name = $"{v.Manufacturer.Name} {v.Model}",
                Manufacturer = v.Manufacturer.Name,
                Model = v.Model,
                ImageUrl = v.ImageUrl ?? DefaultCarImagePath
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} vehicles", result.Count);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Retrieving vehicle with ID: {Id}", id);
            
            var v = await _unitOfWork.VehicleRepository.GetByIdAsync(id);

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
                ImageUrl = v.ImageUrl ?? DefaultCarImagePath,
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
            
            var v = await _unitOfWork.Repository<Vehicle>().FirstOrDefaultAsync(v => v.RegistrationNumber.ToUpper() == regNo.ToUpper());

            if (v is null) 
            {
                _logger.LogWarning("Vehicle with registration number {RegNo} not found", regNo);
                throw new NotFoundException($"Vehicle with registration number {regNo} not found");
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
                ImageUrl = v.ImageUrl ?? DefaultCarImagePath,
                IsSold = v.IsSold
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
            if (await _unitOfWork.Repository<Vehicle>().FirstOrDefaultAsync(v => v.RegistrationNumber.ToUpper() == vehicle.RegistrationNumber.ToUpper()) is not null)
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
                ImageUrl = string.IsNullOrEmpty(vehicle.ImageUrl) ? DefaultCarImageName : vehicle.ImageUrl
            };

            await _unitOfWork.Repository<Vehicle>().AddAsync(vehicleToAdd);

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

            var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(id);
            if (vehicle is null) 
            {
                _logger.LogWarning("Vehicle {Id} not found for update", id);
                throw new NotFoundException($"Vehicle with ID {id} not found");
            }

            // 🔥 VALIDAR REGISTRATION NUMBER SI CAMBIÓ
            if (!string.IsNullOrEmpty(model.RegistrationNumber) && 
                model.RegistrationNumber != vehicle.RegistrationNumber)
            {
                var existingVehicle = await _unitOfWork.Repository<Vehicle>().FirstOrDefaultAsync(v => v.RegistrationNumber.ToUpper() == model.RegistrationNumber.ToUpper());
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
            vehicle.ImageUrl = string.IsNullOrEmpty(model.ImageUrl) ? DefaultCarImageName : model.ImageUrl;

            _unitOfWork.Repository<Vehicle>().Update(vehicle);

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
            
            var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(id);
            if (vehicle is null) 
            {
                _logger.LogWarning("Vehicle {Id} not found for marking as sold", id);
                throw new NotFoundException($"Vehicle with ID {id} not found");
            }

            vehicle.IsSold = true;
            _unitOfWork.Repository<Vehicle>().Update(vehicle);

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
            
            var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(id);
            if (vehicle is null) 
            {
                _logger.LogWarning("Vehicle {Id} not found for deletion", id);
                throw new NotFoundException($"Vehicle with ID {id} not found");
            }

            _unitOfWork.Repository<Vehicle>().Remove(vehicle);

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
            var manufacturer = await _unitOfWork.Repository<Manufacturer>().GetByIdAsync(manufacturerId);
            if (manufacturer is null)
            {
                throw new NotFoundException($"Manufacturer with ID {manufacturerId} not found");
            }

            var fuelType = await _unitOfWork.Repository<FuelType>().GetByIdAsync(fuelTypeId);
            if (fuelType is null)
            {
                throw new NotFoundException($"Fuel type with ID {fuelTypeId} not found");
            }

            var transmissionType = await _unitOfWork.Repository<TransmissionType>().GetByIdAsync(transmissionTypeId);
            if (transmissionType is null)
            {
                throw new NotFoundException($"Transmission type with ID {transmissionTypeId} not found");
            }

            return (manufacturer, fuelType, transmissionType);
        }
    }
}