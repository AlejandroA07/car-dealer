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

        foreach (var preparedVehicle in preparedVehicles)
        {
            var manufacturer = await GetOrCreateManufacturerAsync(preparedVehicle.Manufacturer);
            var fuelType = await GetOrCreateFuelTypeAsync(preparedVehicle.FuelType);
            var transmissionType = await GetOrCreateTransmissionTypeAsync(preparedVehicle.TransmissionType);

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

    private async Task<Manufacturer> GetOrCreateManufacturerAsync(string manufacturerName)
    {
        var normalizedName = NormalizeLookupName(manufacturerName, "Unknown");
        var existingManufacturer = await _unitOfWork.ManufacturerRepository
            .FirstOrDefaultAsync(manufacturer => manufacturer.Name.ToUpper() == normalizedName.ToUpper());

        if (existingManufacturer is not null)
        {
            return existingManufacturer;
        }

        var manufacturer = new Manufacturer { Name = normalizedName };
        await _unitOfWork.ManufacturerRepository.AddAsync(manufacturer);
        return manufacturer;
    }

    private async Task<FuelType> GetOrCreateFuelTypeAsync(string fuelTypeName)
    {
        var normalizedName = NormalizeLookupName(fuelTypeName, "Unknown");
        var existingFuelType = await _unitOfWork.FuelTypeRepository
            .FirstOrDefaultAsync(fuelType => fuelType.Name.ToUpper() == normalizedName.ToUpper());

        if (existingFuelType is not null)
        {
            return existingFuelType;
        }

        var fuelType = new FuelType { Name = normalizedName };
        await _unitOfWork.FuelTypeRepository.AddAsync(fuelType);
        return fuelType;
    }

    private async Task<TransmissionType> GetOrCreateTransmissionTypeAsync(string transmissionTypeName)
    {
        var normalizedName = NormalizeLookupName(transmissionTypeName, "Unknown");
        var existingTransmissionType = await _unitOfWork.TransmissionTypeRepository
            .FirstOrDefaultAsync(transmissionType => transmissionType.Name.ToUpper() == normalizedName.ToUpper());

        if (existingTransmissionType is not null)
        {
            return existingTransmissionType;
        }

        var transmissionType = new TransmissionType { Name = normalizedName };
        await _unitOfWork.TransmissionTypeRepository.AddAsync(transmissionType);
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
}
