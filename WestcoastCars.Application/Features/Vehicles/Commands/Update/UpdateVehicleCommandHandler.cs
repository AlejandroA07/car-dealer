using System.Text.Json;
using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Update;

public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Unit>
{
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
        vehicle.Price = request.Price;
        if (request.IsSold && !vehicle.IsSold) vehicle.MarkAsSold();
        else if (!request.IsSold && vehicle.IsSold) vehicle.MarkAsAvailable();

        // Only update ImageUrl if a new one is provided.
        if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            vehicle.ImageUrl = request.ImageUrl;
        }

        vehicle.Color = request.Color;
        vehicle.WheelDrive = request.WheelDrive;
        vehicle.Horsepower = request.Horsepower;
        vehicle.BodyType = request.BodyType;
        vehicle.Doors = request.Doors;
        vehicle.EngineVolume = request.EngineVolume;
        vehicle.City = request.City;
        vehicle.Address = request.Address;
        vehicle.Equipment = ToJsonArray(request.Equipment);
        vehicle.GalleryUrls = ToJsonArray(request.GalleryUrls);

        _unitOfWork.VehicleRepository.Update(vehicle);

        await _unitOfWork.CompleteOrThrowAsync("Failed to update vehicle");
        return Unit.Value;
    }

    private static string? ToJsonArray(string? newlineSeparated)
    {
        if (string.IsNullOrWhiteSpace(newlineSeparated)) return null;
        var items = newlineSeparated
            .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
        return items.Count > 0 ? JsonSerializer.Serialize(items) : null;
    }
}
