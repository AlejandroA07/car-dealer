using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Update
{
    public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Unit>
    {
        private const string DefaultCarImageName = "/images/no-car.png";
        private readonly IUnitOfWork _unitOfWork;

        public UpdateVehicleCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(request.Id);
            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with ID {request.Id} not found");
            }

            // Validate registration number if it has changed
            if (!string.IsNullOrEmpty(request.RegistrationNumber) && 
                request.RegistrationNumber != vehicle.RegistrationNumber)
            {
                var existingVehicle = await _unitOfWork.VehicleRepository.FindByRegistrationNumberAsync(request.RegistrationNumber);
                if (existingVehicle != null && existingVehicle.Id != request.Id)
                {
                    throw new ConflictException($"Registration number {request.RegistrationNumber} already exists");
                }
                vehicle.RegistrationNumber = request.RegistrationNumber;
            }

            // Validate related entities
            var manufacturer = await _unitOfWork.ManufacturerRepository.GetByIdAsync(request.ManufacturerId);
            if (manufacturer == null) throw new NotFoundException($"Manufacturer with ID {request.ManufacturerId} not found");

            var fuelType = await _unitOfWork.FuelTypeRepository.GetByIdAsync(request.FuelTypeId);
            if (fuelType == null) throw new NotFoundException($"Fuel type with ID {request.FuelTypeId} not found");

            var transmissionType = await _unitOfWork.TransmissionTypeRepository.GetByIdAsync(request.TransmissionTypeId);
            if (transmissionType == null) throw new NotFoundException($"Transmission type with ID {request.TransmissionTypeId} not found");

            // Update properties
            vehicle.Model = request.Model;
            vehicle.ModelYear = request.ModelYear;
            vehicle.Manufacturer = manufacturer;
            vehicle.FuelType = fuelType;
            vehicle.TransmissionType = transmissionType;
            vehicle.Mileage = request.Mileage;
            vehicle.Description = request.Description;
            vehicle.Value = request.Value;
            vehicle.IsSold = request.IsSold;

            // Only update ImageUrl if a new one is provided.
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                vehicle.ImageUrl = request.ImageUrl;
            }

            _unitOfWork.VehicleRepository.Update(vehicle);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return Unit.Value;
            }

            throw new Exception("Failed to update vehicle");
        }
    }
}
