using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Vehicles.Commands.SyncBlocket;

public class SyncBlocketVehiclesCommandHandler : IRequestHandler<SyncBlocketVehiclesCommand, SyncBlocketVehiclesResult>
{
    private const int MaxImportLimit = 50;

    private readonly IBlocketApiClient _blocketApiClient;
    private readonly IBlocketVehicleImportMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public SyncBlocketVehiclesCommandHandler(IBlocketApiClient blocketApiClient, IBlocketVehicleImportMapper mapper, IUnitOfWork unitOfWork)
    {
        _blocketApiClient = blocketApiClient;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<SyncBlocketVehiclesResult> Handle(SyncBlocketVehiclesCommand request, CancellationToken cancellationToken)
    {
        var appliedLimit = NormalizeLimit(request.Limit);
        var preparedVehicles = new List<BlocketVehicleImportData>(appliedLimit);
        var pagesFetched = 0;
        var totalFetched = 0;
        var currentPage = 1;
        var importedAtUtc = DateTime.UtcNow;
        var seenExternalListingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var totalSkipped = 0;

        while (preparedVehicles.Count < appliedLimit)
        {
            var searchResponse = await _blocketApiClient.SearchCarsAsync(new BlocketCarSearchRequest
            {
                Page = currentPage,
                SortOrder = "PUBLISHED_DESC",
                OrgId = request.OrgId,
                Locations = request.Locations,
                Models = request.Models
            }, cancellationToken);

            pagesFetched++;

            if (searchResponse.Docs.Count == 0)
            {
                break;
            }

            totalFetched += searchResponse.Docs.Count;

            foreach (var searchItem in searchResponse.Docs)
            {
                if (preparedVehicles.Count >= appliedLimit)
                {
                    break;
                }

                var adDetails = await _blocketApiClient.GetCarAdAsync(searchItem.Id, cancellationToken);
                var mappedVehicle = _mapper.Map(searchItem, adDetails, importedAtUtc);

                if (string.IsNullOrWhiteSpace(mappedVehicle.ExternalListingId) ||
                    !seenExternalListingIds.Add(mappedVehicle.ExternalListingId))
                {
                    totalSkipped++;
                    continue;
                }

                preparedVehicles.Add(mappedVehicle);
            }

            currentPage++;
        }

        var importedVehicles = await BuildVehicleEntitiesAsync(preparedVehicles);
        var existingVehicles = (await _unitOfWork.VehicleRepository.GetAllAsync()).ToList();

        _unitOfWork.VehicleRepository.RemoveRange(existingVehicles);

        if (importedVehicles.Count > 0)
        {
            await _unitOfWork.VehicleRepository.AddRangeAsync(importedVehicles);
        }

        await _unitOfWork.CompleteAsync();

        return new SyncBlocketVehiclesResult
        {
            RequestedLimit = request.Limit,
            AppliedLimit = appliedLimit,
            PagesFetched = pagesFetched,
            TotalFetched = totalFetched,
            TotalPrepared = preparedVehicles.Count,
            TotalImported = importedVehicles.Count,
            TotalReplaced = existingVehicles.Count,
            TotalSkipped = totalSkipped,
            Vehicles = preparedVehicles
        };
    }

    private async Task<List<Vehicle>> BuildVehicleEntitiesAsync(IEnumerable<BlocketVehicleImportData> preparedVehicles)
    {
        var vehicles = new List<Vehicle>();
        var preparedVehicleList = preparedVehicles as IReadOnlyCollection<BlocketVehicleImportData> ?? preparedVehicles.ToList();

        if (preparedVehicleList.Count == 0)
        {
            return vehicles;
        }

        var manufacturersByName = BuildLookupDictionary(
            await _unitOfWork.ManufacturerRepository.GetAllAsync(),
            manufacturer => manufacturer.Name);
        var fuelTypesByName = BuildLookupDictionary(
            await _unitOfWork.FuelTypeRepository.GetAllAsync(),
            fuelType => fuelType.Name);
        var transmissionTypesByName = BuildLookupDictionary(
            await _unitOfWork.TransmissionTypeRepository.GetAllAsync(),
            transmissionType => transmissionType.Name);

        foreach (var preparedVehicle in preparedVehicleList)
        {
            var manufacturer = await GetOrCreateManufacturerAsync(preparedVehicle.Manufacturer, manufacturersByName);
            var fuelType = await GetOrCreateFuelTypeAsync(preparedVehicle.FuelType, fuelTypesByName);
            var transmissionType = await GetOrCreateTransmissionTypeAsync(preparedVehicle.TransmissionType, transmissionTypesByName);

            vehicles.Add(new Vehicle
            {
                RegistrationNumber = preparedVehicle.RegistrationNumber,
                Model = preparedVehicle.Model,
                ModelYear = preparedVehicle.ModelYear,
                Mileage = preparedVehicle.Mileage,
                ImageUrl = preparedVehicle.ImageUrl,
                Value = preparedVehicle.Value,
                Description = preparedVehicle.Description,
                IsSold = false,
                ExternalListingId = preparedVehicle.ExternalListingId,
                Source = preparedVehicle.Source,
                SourceUrl = preparedVehicle.SourceUrl,
                PublishedAt = preparedVehicle.PublishedAt,
                ImportedAt = preparedVehicle.ImportedAt,
                Color = preparedVehicle.Color,
                City = preparedVehicle.City,
                Manufacturer = manufacturer,
                FuelType = fuelType,
                TransmissionType = transmissionType
            });
        }

        return vehicles;
    }

    private async Task<Manufacturer> GetOrCreateManufacturerAsync(string manufacturerName, IDictionary<string, Manufacturer> manufacturersByName)
    {
        var normalizedName = NormalizeLookupName(manufacturerName, "Unknown");
        if (manufacturersByName.TryGetValue(normalizedName, out var existingManufacturer))
        {
            return existingManufacturer;
        }

        var manufacturer = new Manufacturer { Name = normalizedName };
        await _unitOfWork.ManufacturerRepository.AddAsync(manufacturer);
        manufacturersByName[normalizedName] = manufacturer;
        return manufacturer;
    }

    private async Task<FuelType> GetOrCreateFuelTypeAsync(string fuelTypeName, IDictionary<string, FuelType> fuelTypesByName)
    {
        var normalizedName = NormalizeLookupName(fuelTypeName, "Unknown");
        if (fuelTypesByName.TryGetValue(normalizedName, out var existingFuelType))
        {
            return existingFuelType;
        }

        var fuelType = new FuelType { Name = normalizedName };
        await _unitOfWork.FuelTypeRepository.AddAsync(fuelType);
        fuelTypesByName[normalizedName] = fuelType;
        return fuelType;
    }

    private async Task<TransmissionType> GetOrCreateTransmissionTypeAsync(string transmissionTypeName, IDictionary<string, TransmissionType> transmissionTypesByName)
    {
        var normalizedName = NormalizeLookupName(transmissionTypeName, "Unknown");
        if (transmissionTypesByName.TryGetValue(normalizedName, out var existingTransmissionType))
        {
            return existingTransmissionType;
        }

        var transmissionType = new TransmissionType { Name = normalizedName };
        await _unitOfWork.TransmissionTypeRepository.AddAsync(transmissionType);
        transmissionTypesByName[normalizedName] = transmissionType;
        return transmissionType;
    }

    private static int NormalizeLimit(int requestedLimit)
    {
        if (requestedLimit <= 0)
        {
            return MaxImportLimit;
        }

        return Math.Min(requestedLimit, MaxImportLimit);
    }

    private static string NormalizeLookupName(string? value, string fallbackValue)
    {
        return string.IsNullOrWhiteSpace(value) ? fallbackValue : value.Trim();
    }

    private static Dictionary<string, TLookup> BuildLookupDictionary<TLookup>(
        IEnumerable<TLookup> lookups,
        Func<TLookup, string?> nameSelector)
    {
        var dictionary = new Dictionary<string, TLookup>(StringComparer.OrdinalIgnoreCase);

        foreach (var lookup in lookups)
        {
            var name = NormalizeLookupName(nameSelector(lookup), "Unknown");

            if (!dictionary.ContainsKey(name))
            {
                dictionary.Add(name, lookup);
            }
        }

        return dictionary;
    }
}
