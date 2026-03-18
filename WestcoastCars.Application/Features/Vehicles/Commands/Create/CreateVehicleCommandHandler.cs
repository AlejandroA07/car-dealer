using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Create
{
    public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, int>
    {
        private const string DefaultCarImageName = "no-car.png";
        private readonly IUnitOfWork _unitOfWork;

        public CreateVehicleCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
        {
            // Check if vehicle already exists
            var existing = await _unitOfWork.VehicleRepository.FindByRegistrationNumberAsync(request.RegistrationNumber);
            if (existing != null)
            {
                throw new ConflictException($"Vehicle with registration number {request.RegistrationNumber} already exists");
            }

            // Validate related entities
            var manufacturer = await _unitOfWork.ManufacturerRepository.GetByIdAsync(request.ManufacturerId);
            if (manufacturer == null) throw new NotFoundException($"Manufacturer with ID {request.ManufacturerId} not found");

            var fuelType = await _unitOfWork.FuelTypeRepository.GetByIdAsync(request.FuelTypeId);
            if (fuelType == null) throw new NotFoundException($"Fuel type with ID {request.FuelTypeId} not found");

            var transmissionType = await _unitOfWork.TransmissionTypeRepository.GetByIdAsync(request.TransmissionTypeId);
            if (transmissionType == null) throw new NotFoundException($"Transmission type with ID {request.TransmissionTypeId} not found");

            var vehicle = new Vehicle
            {
                RegistrationNumber = request.RegistrationNumber,
                Manufacturer = manufacturer,
                Model = request.Model,
                ModelYear = request.ModelYear,
                Mileage = request.Mileage,
                TransmissionType = transmissionType,
                FuelType = fuelType,
                Value = request.Value,
                IsSold = request.IsSold,
                Description = request.Description,
                ImageUrl = string.IsNullOrEmpty(request.ImageUrl) ? DefaultCarImageName : request.ImageUrl
            };

            await _unitOfWork.VehicleRepository.AddAsync(vehicle);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return vehicle.Id;
            }

            throw new Exception("Failed to create vehicle");
        }
    }
}
