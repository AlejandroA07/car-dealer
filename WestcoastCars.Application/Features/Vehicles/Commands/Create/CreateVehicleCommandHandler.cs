using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Create;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, int>
{
    private const string DefaultCarImageName = "/images/no-car.png";
    private readonly IUnitOfWork _unitOfWork;

    public CreateVehicleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
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

        await _unitOfWork.CompleteOrThrowAsync("Failed to create vehicle");
        return vehicle.Id;
    }
}
