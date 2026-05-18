using System.Text.Json;
using AutoMapper;
using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Create;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, VehicleDetailsDto>
{
    private const string DefaultCarImageName = "/images/no-car.png";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateVehicleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VehicleDetailsDto> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
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
            Equipment = ToJsonArray(request.Equipment),
            GalleryUrls = ToJsonArray(request.GalleryUrls)
        };

        if (request.IsSold) vehicle.MarkAsSold();

        await _unitOfWork.VehicleRepository.AddAsync(vehicle);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create vehicle");
        return _mapper.Map<VehicleDetailsDto>(vehicle);
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
