using System.Text.Json;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Vehicles.Commands.ImportSelectedBlocketVehicles;

public class ImportSelectedBlocketVehiclesCommandHandler(IBlocketApiClient blocketApiClient, IBlocketVehicleImportMapper mapper, IUnitOfWork unitOfWork) : IRequestHandler<ImportSelectedBlocketVehiclesCommand, ImportSelectedResult>
{
    private readonly IBlocketApiClient _blocketApiClient = blocketApiClient;
    private readonly IBlocketVehicleImportMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ImportSelectedResult> Handle(ImportSelectedBlocketVehiclesCommand request, CancellationToken cancellationToken)
    {
        var importedAtUtc = DateTime.UtcNow;
        var preparedVehicles = new List<BlocketVehicleImportData>(request.ExternalListingIds.Count);
        var totalSkipped = 0;

        foreach (var externalId in request.ExternalListingIds)
        {
            if (string.IsNullOrWhiteSpace(externalId))
            {
                totalSkipped++;
                continue;
            }

            var adDetails = await _blocketApiClient.GetCarAdAsync(externalId, cancellationToken);
            var knownImageUrl = request.ImageUrlsById.TryGetValue(externalId, out var url) ? url : null;
            if (!string.IsNullOrWhiteSpace(knownImageUrl) && adDetails.Image is null && adDetails.Images.Count == 0)
                adDetails.Image = new BlocketImage { Url = knownImageUrl };
            var pseudo = new BlocketCarSearchItem { Id = externalId };
            var mapped = _mapper.Map(pseudo, adDetails, importedAtUtc);

            if (!mapped.ModelYear.HasValue)
            {
                totalSkipped++;
                continue;
            }

            preparedVehicles.Add(mapped);
        }

        var existingByExternalId = (await _unitOfWork.VehicleRepository.GetAllImportedFromBlocketAsync())
            .Where(v => !string.IsNullOrWhiteSpace(v.ExternalListingId))
            .ToDictionary(v => v.ExternalListingId!, StringComparer.OrdinalIgnoreCase);

        var newVehicles = new List<BlocketVehicleImportData>();
        var updatedCount = 0;

        foreach (var prepared in preparedVehicles)
        {
            if (existingByExternalId.TryGetValue(prepared.ExternalListingId!, out var existing))
            {
                existing.Price = prepared.Price;
                existing.Mileage = prepared.Mileage;
                existing.ImportedAt = prepared.ImportedAt;
                existing.Color = prepared.Color;
                existing.WheelDrive = prepared.WheelDrive;
                existing.Horsepower = prepared.Horsepower;
                existing.BodyType = prepared.BodyType;
                existing.Doors = prepared.Doors;
                existing.EngineVolume = prepared.EngineVolume;
                existing.City = prepared.City;
                existing.Address = prepared.Address;
                existing.Equipment = prepared.Equipment.Count > 0 ? JsonSerializer.Serialize(prepared.Equipment) : null;
                existing.GalleryUrls = prepared.GalleryUrls.Count > 0 ? JsonSerializer.Serialize(prepared.GalleryUrls) : null;
                existing.Seats = prepared.Seats;
                existing.MaxTrailerWeight = prepared.MaxTrailerWeight;
                existing.OwnerCount = prepared.OwnerCount;
                existing.LastInspectionDate = prepared.LastInspectionDate;
                existing.NextInspectionDate = prepared.NextInspectionDate;
                _unitOfWork.VehicleRepository.Update(existing);
                updatedCount++;
            }
            else
            {
                newVehicles.Add(prepared);
            }
        }

        var addedVehicles = await BuildVehicleEntitiesAsync(newVehicles);
        if (addedVehicles.Count > 0)
        {
            await _unitOfWork.VehicleRepository.AddRangeAsync(addedVehicles);
        }

        await _unitOfWork.CompleteAsync();

        return new ImportSelectedResult
        {
            TotalSelected = request.ExternalListingIds.Count,
            TotalAdded = addedVehicles.Count,
            TotalUpdated = updatedCount,
            TotalSkipped = totalSkipped
        };
    }

    private async Task<List<Vehicle>> BuildVehicleEntitiesAsync(IEnumerable<BlocketVehicleImportData> preparedVehicles)
    {
        var vehicles = new List<Vehicle>();
        var list = preparedVehicles as IReadOnlyCollection<BlocketVehicleImportData> ?? [.. preparedVehicles];

        if (list.Count == 0) return vehicles;

        var manufacturersByName = BuildLookupDictionary(
            await _unitOfWork.ManufacturerRepository.GetAllAsync(), m => m.Name);
        var fuelTypesByName = BuildLookupDictionary(
            await _unitOfWork.FuelTypeRepository.GetAllAsync(), f => f.Name);
        var transmissionTypesByName = BuildLookupDictionary(
            await _unitOfWork.TransmissionTypeRepository.GetAllAsync(), t => t.Name);

        foreach (var prepared in list)
        {
            var manufacturer = await GetOrCreateLookupAsync(
                prepared.Manufacturer, manufacturersByName,
                name => new Manufacturer { Name = name }, _unitOfWork.ManufacturerRepository);
            var fuelType = await GetOrCreateLookupAsync(
                prepared.FuelType, fuelTypesByName,
                name => new FuelType { Name = name }, _unitOfWork.FuelTypeRepository);
            var transmissionType = await GetOrCreateLookupAsync(
                prepared.TransmissionType, transmissionTypesByName,
                name => new TransmissionType { Name = name }, _unitOfWork.TransmissionTypeRepository);

            vehicles.Add(new Vehicle
            {
                RegistrationNumber = prepared.RegistrationNumber,
                Model = prepared.Model,
                ModelYear = prepared.ModelYear!.Value,
                Mileage = prepared.Mileage,
                ImageUrl = prepared.ImageUrl,
                Price = prepared.Price,
                Description = prepared.Description,
                ExternalListingId = prepared.ExternalListingId,
                Source = prepared.Source,
                SourceUrl = prepared.SourceUrl,
                PublishedAt = prepared.PublishedAt,
                ImportedAt = prepared.ImportedAt,
                Color = prepared.Color,
                WheelDrive = prepared.WheelDrive,
                Horsepower = prepared.Horsepower,
                BodyType = prepared.BodyType,
                Doors = prepared.Doors,
                EngineVolume = prepared.EngineVolume,
                City = prepared.City,
                Address = prepared.Address,
                Equipment = prepared.Equipment.Count > 0
                    ? JsonSerializer.Serialize(prepared.Equipment)
                    : null,
                GalleryUrls = prepared.GalleryUrls.Count > 0
                    ? JsonSerializer.Serialize(prepared.GalleryUrls)
                    : null,
                Seats = prepared.Seats,
                MaxTrailerWeight = prepared.MaxTrailerWeight,
                OwnerCount = prepared.OwnerCount,
                LastInspectionDate = prepared.LastInspectionDate,
                NextInspectionDate = prepared.NextInspectionDate,
                Manufacturer = manufacturer,
                FuelType = fuelType,
                TransmissionType = transmissionType
            });
        }

        return vehicles;
    }

    private static async Task<TLookup> GetOrCreateLookupAsync<TLookup>(
        string lookupName,
        IDictionary<string, TLookup> lookupsByName,
        Func<string, TLookup> factory,
        IRepository<TLookup> repository)
        where TLookup : class
    {
        var name = NormalizeLookupName(lookupName);
        if (lookupsByName.TryGetValue(name, out var existing)) return existing;
        var created = factory(name);
        await repository.AddAsync(created);
        lookupsByName[name] = created;
        return created;
    }

    private static Dictionary<string, TLookup> BuildLookupDictionary<TLookup>(
        IEnumerable<TLookup> lookups,
        Func<TLookup, string?> nameSelector)
    {
        var dict = new Dictionary<string, TLookup>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in lookups)
        {
            var name = NormalizeLookupName(nameSelector(item));
            dict.TryAdd(name, item);
        }
        return dict;
    }

    private static string NormalizeLookupName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
}
