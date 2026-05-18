using MediatR;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Helpers;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Models.Blocket;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.PreviewBlocketVehicles;

public class PreviewBlocketVehiclesQueryHandler : IRequestHandler<PreviewBlocketVehiclesQuery, List<BlocketPreviewDto>>
{
    private readonly IBlocketApiClient _blocketApiClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PreviewBlocketVehiclesQueryHandler> _logger;

    public PreviewBlocketVehiclesQueryHandler(IBlocketApiClient blocketApiClient, IUnitOfWork unitOfWork, ILogger<PreviewBlocketVehiclesQueryHandler> logger)
    {
        _blocketApiClient = blocketApiClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private const int MaxPages = 30;

    public async Task<List<BlocketPreviewDto>> Handle(PreviewBlocketVehiclesQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Min(Math.Max(request.Limit, 1), 50);
        var results = new List<BlocketPreviewDto>(limit);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentPage = 1;
        var skipNoRegNo = 0; var skipYear = 0; var skipMileage = 0; var skipTransmission = 0; var skipFuel = 0;

        var existingIds = (await _unitOfWork.VehicleRepository.GetAllImportedFromBlocketAsync())
            .Where(v => !string.IsNullOrWhiteSpace(v.ExternalListingId))
            .Select(v => v.ExternalListingId!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        while (results.Count < limit && currentPage <= MaxPages)
        {
            var searchResponse = await _blocketApiClient.SearchCarsAsync(new BlocketCarSearchRequest
            {
                Query = request.Query,
                Page = currentPage,
                SortOrder = request.SortOrder ?? "PUBLISHED_DESC",
                OrgId = request.OrgId,
                Locations = request.Locations,
                Models = request.Manufacturers,
                PriceFrom = request.PriceFrom,
                PriceTo = request.PriceTo,
                YearFrom = request.YearFrom,
                YearTo = request.YearTo,
                MilageFrom = request.MinMileage.HasValue ? request.MinMileage / 10 : null,
                MilageTo = request.MaxMileage.HasValue ? request.MaxMileage / 10 : null,
                Colors = request.Colors,
                Transmissions = ToApiTransmission(request.TransmissionFilter),
                WheelDrive = request.WheelDrive,
                HorsepowerFrom = request.HorsepowerFrom,
                HorsepowerTo = request.HorsepowerTo
            }, cancellationToken);

            if (searchResponse.Docs.Count == 0) break;

            foreach (var item in searchResponse.Docs)
            {
                if (results.Count >= limit) break;

                if (string.IsNullOrWhiteSpace(item.Id) || !seen.Add(item.Id)) continue;
                if (existingIds.Contains(item.Id)) continue;
                if (!IsValidModelYear(item.Year)) { skipYear++; continue; }
                if (!BlocketFilterHelpers.PassesMileageFilter(item, request.MinMileage, request.MaxMileage)) { skipMileage++; continue; }
                if (!BlocketFilterHelpers.PassesTransmissionFilter(item.Transmission, request.TransmissionFilter)) { skipTransmission++; continue; }
                if (!BlocketFilterHelpers.PassesFuelFilter(item.Fuel, request.FuelTypeFilter)) { skipFuel++; continue; }

                results.Add(new BlocketPreviewDto
                {
                    ExternalListingId = item.Id,
                    Title = item.Heading,
                    Make = item.Make,
                    Model = item.Model,
                    Year = item.Year,
                    MileageKm = item.Mileage.HasValue
                        ? BlocketFilterHelpers.NormalizeMileageKm(item.Mileage.Value, item.MileageUnit)
                        : null,
                    Price = item.Price?.Amount,
                    ImageUrl = item.Image?.Url,
                    Location = item.Location,
                    RegistrationNumber = item.RegistrationNumber,
                    SourceUrl = item.CanonicalUrl,
                    Transmission = item.Transmission,
                    Fuel = item.Fuel,
                    IsAlreadyImported = existingIds.Contains(item.Id)
                });
            }

            currentPage++;
        }

        _logger.LogDebug(
            "Preview filter summary — noRegNo:{NoRegNo} badYear:{Year} mileage:{Mileage} transmission:{Transmission} fuel:{Fuel} found:{Found}",
            skipNoRegNo, skipYear, skipMileage, skipTransmission, skipFuel, results.Count);

        return results;
    }

    private static string? ToApiTransmission(string? filter) => filter switch
    {
        "Automat" => "AUTOMATIC",
        "Manuell" => "MANUAL",
        _ => null
    };

    private static bool IsValidModelYear(int? year) =>
        year.HasValue && year.Value >= 1900 && year.Value <= DateTime.UtcNow.Year + 1;
}
