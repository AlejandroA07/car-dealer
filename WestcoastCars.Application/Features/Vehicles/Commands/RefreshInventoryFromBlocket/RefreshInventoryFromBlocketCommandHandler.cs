using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Vehicles.Commands.RefreshInventoryFromBlocket;

public class RefreshInventoryFromBlocketCommandHandler : IRequestHandler<RefreshInventoryFromBlocketCommand, RefreshInventoryFromBlocketResult>
{
    private const int MaxImportLimit = 50;

    private readonly IBlocketApiClient _blocketApiClient;
    private readonly IBlocketVehicleImportMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshInventoryFromBlocketCommandHandler(IBlocketApiClient blocketApiClient, IBlocketVehicleImportMapper mapper, IUnitOfWork unitOfWork)
    {
        _blocketApiClient = blocketApiClient;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshInventoryFromBlocketResult> Handle(RefreshInventoryFromBlocketCommand request, CancellationToken cancellationToken)
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
                Models = request.Manufacturers,
                YearFrom = request.YearFrom,
                YearTo = request.YearTo,
                PriceFrom = request.PriceFrom,
                PriceTo = request.PriceTo
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

                // Intentionally sequential: BlocketApiClient applies a shared process-wide throttle,
                // so parallel detail requests would only add contention and risk upstream pressure.
                var adDetails = await _blocketApiClient.GetCarAdAsync(searchItem.Id, cancellationToken);
                var mappedVehicle = _mapper.Map(searchItem, adDetails, importedAtUtc);

                if (string.IsNullOrWhiteSpace(mappedVehicle.ExternalListingId) ||
                    !seenExternalListingIds.Add(mappedVehicle.ExternalListingId) ||
                    string.IsNullOrWhiteSpace(mappedVehicle.RegistrationNumber) ||
                    !IsValidModelYear(mappedVehicle.ModelYear) ||
                    (request.MinMileage.HasValue && mappedVehicle.Mileage < request.MinMileage.Value) ||
                    (request.MaxMileage.HasValue && mappedVehicle.Mileage >= request.MaxMileage.Value) ||
                    (!string.IsNullOrWhiteSpace(request.TransmissionFilter) && !mappedVehicle.TransmissionType.Equals(request.TransmissionFilter, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(request.FuelTypeFilter) && !mappedVehicle.FuelType.Equals(request.FuelTypeFilter, StringComparison.OrdinalIgnoreCase)))
                {
                    totalSkipped++;
                    continue;
                }

                preparedVehicles.Add(mappedVehicle);
            }

            currentPage++;
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

        var removedAtUtc = DateTime.UtcNow;
        var flaggedCount = 0;
        foreach (var (extId, vehicle) in existingByExternalId)
        {
            if (!seenExternalListingIds.Contains(extId))
            {
                vehicle.MarkAsSourceRemoved(removedAtUtc);
                _unitOfWork.VehicleRepository.Update(vehicle);
                flaggedCount++;
            }
        }

        await _unitOfWork.CompleteAsync();

        return new RefreshInventoryFromBlocketResult
        {
            RequestedLimit = request.Limit,
            AppliedLimit = appliedLimit,
            PagesFetched = pagesFetched,
            TotalFetched = totalFetched,
            TotalPrepared = preparedVehicles.Count,
            TotalAdded = addedVehicles.Count,
            TotalUpdated = updatedCount,
            TotalFlagged = flaggedCount,
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
            var manufacturer = await GetOrCreateLookupAsync(
                preparedVehicle.Manufacturer,
                manufacturersByName,
                name => new Manufacturer { Name = name },
                _unitOfWork.ManufacturerRepository);
            var fuelType = await GetOrCreateLookupAsync(
                preparedVehicle.FuelType,
                fuelTypesByName,
                name => new FuelType { Name = name },
                _unitOfWork.FuelTypeRepository);
            var transmissionType = await GetOrCreateLookupAsync(
                preparedVehicle.TransmissionType,
                transmissionTypesByName,
                name => new TransmissionType { Name = name },
                _unitOfWork.TransmissionTypeRepository);

            vehicles.Add(new Vehicle
            {
                RegistrationNumber = preparedVehicle.RegistrationNumber!,
                Model = preparedVehicle.Model,
                ModelYear = preparedVehicle.ModelYear.Value,
                Mileage = preparedVehicle.Mileage,
                ImageUrl = preparedVehicle.ImageUrl,
                Price = preparedVehicle.Price,
                Description = preparedVehicle.Description,
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

    private static async Task<TLookup> GetOrCreateLookupAsync<TLookup>(
        string lookupName,
        IDictionary<string, TLookup> lookupsByName,
        Func<string, TLookup> factory,
        IRepository<TLookup> repository)
        where TLookup : class
    {
        var normalizedName = NormalizeLookupName(lookupName, "Unknown");
        if (lookupsByName.TryGetValue(normalizedName, out var existingLookup))
        {
            return existingLookup;
        }

        var lookup = factory(normalizedName);
        await repository.AddAsync(lookup);
        lookupsByName[normalizedName] = lookup;
        return lookup;
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

    private static bool IsValidModelYear(int? modelYear) =>
        modelYear.HasValue &&
        modelYear.Value >= 1900 &&
        modelYear.Value <= DateTime.UtcNow.Year + 1;

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
