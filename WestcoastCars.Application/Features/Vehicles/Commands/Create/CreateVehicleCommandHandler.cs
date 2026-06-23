using MediatR;
using WestcoastCars.Application.Common.Helpers;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Create;

public class CreateVehicleCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateVehicleCommand, VehicleDetailsDto>
{
    private const string DefaultCarImageName = "/images/no-car.png";
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<VehicleDetailsDto> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var manufacturer = await _unitOfWork.ManufacturerRepository.GetByIdAsync(request.ManufacturerId) ?? throw new NotFoundException($"Manufacturer with ID {request.ManufacturerId} not found");
        var fuelType = await _unitOfWork.FuelTypeRepository.GetByIdAsync(request.FuelTypeId) ?? throw new NotFoundException($"Fuel type with ID {request.FuelTypeId} not found");
        var transmissionType = await _unitOfWork.TransmissionTypeRepository.GetByIdAsync(request.TransmissionTypeId) ?? throw new NotFoundException($"Transmission type with ID {request.TransmissionTypeId} not found");
        var vehicle = new Vehicle
        {
            RegistrationNumber = request.RegistrationNumber,
            Manufacturer = manufacturer,
            Model = request.Model,
            ModelYear = request.ModelYear,
            Mileage = request.Mileage,
            TransmissionType = transmissionType,
            FuelType = fuelType,
            Price = request.Price,
            Description = request.Description,
            ImageUrl = string.IsNullOrEmpty(request.ImageUrl) ? DefaultCarImageName : request.ImageUrl,
            Color = request.Color,
            WheelDrive = request.WheelDrive,
            Horsepower = request.Horsepower,
            BodyType = request.BodyType,
            Doors = request.Doors,
            EngineVolume = request.EngineVolume,
            City = request.City,
            Address = request.Address,
            Seats = request.Seats,
            MaxTrailerWeight = request.MaxTrailerWeight,
            OwnerCount = request.OwnerCount,
            LastInspectionDate = request.LastInspectionDate,
            NextInspectionDate = request.NextInspectionDate,
            Equipment = VehicleFieldSerializer.ToJsonArray(request.Equipment),
            GalleryUrls = VehicleFieldSerializer.ToJsonArray(request.GalleryUrls)
        };

        if (request.IsSold) vehicle.MarkAsSold();

        await _unitOfWork.VehicleRepository.AddAsync(vehicle);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create vehicle");
        return vehicle.ToDetailsDto();
    }
}
