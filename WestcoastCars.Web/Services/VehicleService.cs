using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using WestcoastCars.Web.ViewModels.Vehicles;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.Services;

public class VehicleService : IVehicleService
{
    private const string ManufacturersCacheKey = "vehicle-form-manufacturers";
    private const string FuelTypesCacheKey = "vehicle-form-fuel-types";
    private const string TransmissionsCacheKey = "vehicle-form-transmissions";
    private static readonly TimeSpan DropdownCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly HttpClient _longRunningHttpClient;
    private readonly ILogger<VehicleService> _logger;
    private readonly string _baseUrl;
    private readonly IMemoryCache _cache;

    public VehicleService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<VehicleService> logger, IMemoryCache cache)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _longRunningHttpClient = httpClientFactory.CreateClient("LongRunningApiClient");
        _logger = logger;
        _baseUrl = config["Services:ApiUrl"] ?? throw new InvalidOperationException("Services:ApiUrl is not configured");
        _cache = cache;
    }

    public async Task<PagedResult<VehicleSummaryDto>> ListVehiclesAsync(int page = 1, int pageSize = 20)
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var url = QueryHelpers.AddQueryString($"{_baseUrl}/api/v1/vehicles/list", new Dictionary<string, string?>
            {
                ["Page"] = page.ToString(),
                ["PageSize"] = pageSize.ToString()
            });
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching vehicle list: {StatusCode}", response.StatusCode);
                return new PagedResult<VehicleSummaryDto> { Page = page, PageSize = pageSize };
            }

            return await response.Content.ReadFromJsonAsync<PagedResult<VehicleSummaryDto>>(JsonOptions)
                ?? new PagedResult<VehicleSummaryDto> { Page = page, PageSize = pageSize };
        }, new PagedResult<VehicleSummaryDto> { Page = page, PageSize = pageSize }, "listing vehicles");
    }

    public async Task<List<VehicleSummaryDto>> ListAllVehiclesAsync()
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var firstPageUrl = QueryHelpers.AddQueryString($"{_baseUrl}/api/v1/vehicles/list-all", new Dictionary<string, string?>
            {
                ["Page"] = "1",
                ["PageSize"] = "100"
            });
            var response = await _httpClient.GetAsync(firstPageUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching all vehicles list: {StatusCode}", response.StatusCode);
                return new List<VehicleSummaryDto>();
            }

            var firstPage = await response.Content.ReadFromJsonAsync<PagedResult<VehicleSummaryDto>>(JsonOptions);
            if (firstPage is null)
            {
                return new List<VehicleSummaryDto>();
            }

            var vehicles = new List<VehicleSummaryDto>(firstPage.Items);
            var totalPages = firstPage.PageSize <= 0
                ? 1
                : (int)Math.Ceiling(firstPage.TotalCount / (double)firstPage.PageSize);

            for (var currentPage = 2; currentPage <= totalPages; currentPage++)
            {
                var pageUrl = QueryHelpers.AddQueryString($"{_baseUrl}/api/v1/vehicles/list-all", new Dictionary<string, string?>
                {
                    ["Page"] = currentPage.ToString(),
                    ["PageSize"] = firstPage.PageSize.ToString()
                });

                var pageResponse = await _httpClient.GetAsync(pageUrl);
                if (!pageResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Error fetching vehicle list page {Page}: {StatusCode}", currentPage, pageResponse.StatusCode);
                    break;
                }

                var pageResult = await pageResponse.Content.ReadFromJsonAsync<PagedResult<VehicleSummaryDto>>(JsonOptions);
                if (pageResult is null)
                {
                    break;
                }

                vehicles.AddRange(pageResult.Items);
            }

            return vehicles;
        }, new List<VehicleSummaryDto>(), "listing all vehicles");
    }

    public async Task<VehicleDetailsDto?> GetVehicleByIdAsync(int id)
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error fetching vehicle {VehicleId}: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<VehicleDetailsDto>(JsonOptions);
        }, null, $"getting vehicle {id}");
    }

    public async Task<(bool Seeded, string Message)> SeedVehiclesAsync()
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/seed/vehicles", null);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Seed vehicles failed: {StatusCode}", response.StatusCode);
                return (false, "Inläsning av fordonsdata misslyckades.");
            }
            var result = await response.Content.ReadFromJsonAsync<SeedVehiclesResponse>(JsonOptions);
            return (result?.Seeded ?? false, result?.Message ?? string.Empty);
        }, (false, "API:t kunde inte nås."), "seeding vehicles");
    }

    public async Task<bool> MarkAsSoldAsync(int id)
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var response = await _httpClient.PatchAsync($"{_baseUrl}/api/v1/vehicles/{id}", null);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Vehicle {VehicleId} marked as sold successfully", id);
                return true;
            }
            _logger.LogError("Error marking vehicle {VehicleId} as sold: {StatusCode}", id, response.StatusCode);
            return false;
        }, false, $"marking vehicle {id} as sold");
    }

    public async Task<bool> DeleteVehicleAsync(int id)
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/v1/vehicles/{id}");
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Vehicle {VehicleId} deleted successfully", id);
                return true;
            }

            _logger.LogError("Error deleting vehicle {VehicleId}: {StatusCode}", id, response.StatusCode);
            return false;
        }, false, $"deleting vehicle {id}");
    }

    public async Task<VehicleBaseViewModel?> GetVehicleForEditAsync(int id)
    {
        var vehicleToEdit = await GetVehicleByIdAsync(id);
        if (vehicleToEdit is null) return null;

        var viewModel = new VehicleBaseViewModel
        {
            Vehicle = new VehicleDto
            {
                Id = vehicleToEdit.Id,
                RegistrationNumber = vehicleToEdit.RegistrationNumber ?? string.Empty,
                Model = vehicleToEdit.Model,
                ModelYear = vehicleToEdit.ModelYear,
                Mileage = vehicleToEdit.Mileage,
                Price = vehicleToEdit.Price,
                Description = vehicleToEdit.Description,
                IsSold = vehicleToEdit.IsSold,
                ImageUrl = vehicleToEdit.ImageUrl,
                Color = vehicleToEdit.Color,
                WheelDrive = vehicleToEdit.WheelDrive,
                Horsepower = vehicleToEdit.Horsepower,
                BodyType = vehicleToEdit.BodyType,
                Doors = vehicleToEdit.Doors,
                EngineVolume = vehicleToEdit.EngineVolume,
                City = vehicleToEdit.City,
                Address = vehicleToEdit.Address,
                Seats = vehicleToEdit.Seats,
                MaxTrailerWeight = vehicleToEdit.MaxTrailerWeight,
                OwnerCount = vehicleToEdit.OwnerCount,
                LastInspectionDate = vehicleToEdit.LastInspectionDate,
                NextInspectionDate = vehicleToEdit.NextInspectionDate,
                Equipment = vehicleToEdit.Equipment.Count > 0 ? string.Join("\n", vehicleToEdit.Equipment) : null,
                GalleryUrls = vehicleToEdit.ImageUrls.Count > 0 ? string.Join("\n", vehicleToEdit.ImageUrls) : null
            }
        };

        await LoadDropdownData(viewModel, vehicleToEdit);
        return viewModel;
    }

    public async Task<bool> UpdateVehicleAsync(int id, VehicleDto vehicle)
    {
        var updateDto = new VehicleUpdateDto
        {
            Id = vehicle.Id,
            Model = vehicle.Model,
            ModelYear = vehicle.ModelYear,
            Mileage = vehicle.Mileage,
            Description = vehicle.Description,
            Price = vehicle.Price,
            IsSold = vehicle.IsSold,
            ImageUrl = vehicle.ImageUrl,
            ManufacturerId = vehicle.ManufacturerId,
            FuelTypeId = vehicle.FuelTypeId,
            TransmissionTypeId = vehicle.TransmissionTypeId,
            RegistrationNumber = vehicle.RegistrationNumber,
            Color = vehicle.Color,
            WheelDrive = vehicle.WheelDrive,
            Horsepower = vehicle.Horsepower,
            BodyType = vehicle.BodyType,
            Doors = vehicle.Doors,
            EngineVolume = vehicle.EngineVolume,
            City = vehicle.City,
            Address = vehicle.Address,
            Seats = vehicle.Seats,
            MaxTrailerWeight = vehicle.MaxTrailerWeight,
            OwnerCount = vehicle.OwnerCount,
            LastInspectionDate = vehicle.LastInspectionDate,
            NextInspectionDate = vehicle.NextInspectionDate,
            Equipment = vehicle.Equipment,
            GalleryUrls = vehicle.GalleryUrls
        };

        return await ExecuteWithApiFallback(async () =>
        {
            var jsonPayload = JsonSerializer.Serialize(updateDto, JsonOptions);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/v1/vehicles/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Vehicle {VehicleId} updated successfully", id);
                return true;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error updating vehicle {VehicleId}: {StatusCode} - {ResponseContent}", id, response.StatusCode, responseContent);
            return false;
        }, false, $"updating vehicle {id}");
    }

    public async Task<VehicleBaseViewModel> GetVehicleForCreateAsync()
    {
        var viewModel = new VehicleBaseViewModel
        {
            Vehicle = new VehicleDto()
        };
        await LoadDropdownDataSimple(viewModel);
        return viewModel;
    }

    public async Task<bool> CreateVehicleAsync(VehicleBaseViewModel vehicleViewModel)
    {
        var postDto = new VehiclePostDto
        {
            RegistrationNumber = vehicleViewModel.Vehicle.RegistrationNumber,
            ManufacturerId = vehicleViewModel.Vehicle.ManufacturerId,
            Model = vehicleViewModel.Vehicle.Model,
            ModelYear = vehicleViewModel.Vehicle.ModelYear,
            Mileage = vehicleViewModel.Vehicle.Mileage,
            FuelTypeId = vehicleViewModel.Vehicle.FuelTypeId,
            TransmissionTypeId = vehicleViewModel.Vehicle.TransmissionTypeId,
            Price = vehicleViewModel.Vehicle.Price,
            Description = vehicleViewModel.Vehicle.Description,
            IsSold = vehicleViewModel.Vehicle.IsSold,
            ImageUrl = vehicleViewModel.Vehicle.ImageUrl,
            Color = vehicleViewModel.Vehicle.Color,
            WheelDrive = vehicleViewModel.Vehicle.WheelDrive,
            Horsepower = vehicleViewModel.Vehicle.Horsepower,
            BodyType = vehicleViewModel.Vehicle.BodyType,
            Doors = vehicleViewModel.Vehicle.Doors,
            EngineVolume = vehicleViewModel.Vehicle.EngineVolume,
            City = vehicleViewModel.Vehicle.City,
            Address = vehicleViewModel.Vehicle.Address,
            Seats = vehicleViewModel.Vehicle.Seats,
            MaxTrailerWeight = vehicleViewModel.Vehicle.MaxTrailerWeight,
            OwnerCount = vehicleViewModel.Vehicle.OwnerCount,
            LastInspectionDate = vehicleViewModel.Vehicle.LastInspectionDate,
            NextInspectionDate = vehicleViewModel.Vehicle.NextInspectionDate,
            Equipment = vehicleViewModel.Vehicle.Equipment,
            GalleryUrls = vehicleViewModel.Vehicle.GalleryUrls
        };

        return await ExecuteWithApiFallback(async () =>
        {
            var jsonPayload = JsonSerializer.Serialize(postDto, JsonOptions);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/vehicles", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Vehicle created successfully");
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error creating vehicle: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
            return false;
        }, false, "creating vehicle");
    }

    public async Task<PagedResult<VehicleSummaryDto>> SearchVehiclesAsync(VehicleSearchDto search)
    {
        var queryParams = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(search.Make)) queryParams["Make"] = search.Make;
        if (!string.IsNullOrWhiteSpace(search.Model)) queryParams["Model"] = search.Model;
        if (search.MinYear.HasValue) queryParams["MinYear"] = search.MinYear.Value.ToString();
        if (search.MaxYear.HasValue) queryParams["MaxYear"] = search.MaxYear.Value.ToString();
        if (search.MinPrice.HasValue) queryParams["MinPrice"] = search.MinPrice.Value.ToString();
        if (search.MaxPrice.HasValue) queryParams["MaxPrice"] = search.MaxPrice.Value.ToString();
        if (search.MinMileage.HasValue) queryParams["MinMileage"] = search.MinMileage.Value.ToString();
        if (search.MaxMileage.HasValue) queryParams["MaxMileage"] = search.MaxMileage.Value.ToString();
        if (search.IsSold.HasValue) queryParams["IsSold"] = search.IsSold.Value.ToString();
        queryParams["Page"] = search.Page.ToString();
        queryParams["PageSize"] = search.PageSize.ToString();

        return await ExecuteWithApiFallback(async () =>
        {
            var url = QueryHelpers.AddQueryString($"{_baseUrl}/api/v1/vehicles/search", queryParams);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error searching vehicles: {StatusCode}", response.StatusCode);
                return new PagedResult<VehicleSummaryDto> { Page = search.Page, PageSize = search.PageSize };
            }

            return await response.Content.ReadFromJsonAsync<PagedResult<VehicleSummaryDto>>(JsonOptions)
                ?? new PagedResult<VehicleSummaryDto> { Page = search.Page, PageSize = search.PageSize };
        }, new PagedResult<VehicleSummaryDto> { Page = search.Page, PageSize = search.PageSize }, "searching vehicles");
    }

    public async Task<BlocketSyncViewModel> SyncBlocketAsync(BlocketSyncViewModel model)
    {
        try
        {
            (int? minMileage, int? maxMileage) = model.MileageBand switch
            {
                "0-10000"     => (0,     (int?)10000),
                "10000-20000" => (10000, (int?)20000),
                "20000-30000" => (20000, (int?)30000),
                "30000-40000" => (30000, (int?)40000),
                "40000-"      => (40000, (int?)null),
                _             => ((int?)null, (int?)null)
            };

            var response = await _longRunningHttpClient.PostAsJsonAsync($"{_baseUrl}/api/v1/vehicles/import/blocket", new
            {
                limit = model.Limit,
                orgId = model.OrgId,
                locations = model.Locations,
                models = model.Manufacturers,
                minMileage,
                maxMileage,
                transmissionFilter = string.IsNullOrWhiteSpace(model.TransmissionFilter) ? null : model.TransmissionFilter,
                fuelTypeFilter = string.IsNullOrWhiteSpace(model.FuelTypeFilter) ? null : model.FuelTypeFilter,
                yearFrom = model.YearFrom,
                yearTo = model.YearTo,
                priceFrom = model.PriceFrom,
                priceTo = model.PriceTo
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error syncing Blocket vehicles: {StatusCode} - {Error}", response.StatusCode, errorContent);
                var errorMessage = "The sync could not be completed. Please try again.";

                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    try
                    {
                        using var jsonDocument = JsonDocument.Parse(errorContent);
                        if (jsonDocument.RootElement.TryGetProperty("detail", out var detailElement) &&
                            detailElement.ValueKind == JsonValueKind.String &&
                            !string.IsNullOrWhiteSpace(detailElement.GetString()))
                        {
                            errorMessage = detailElement.GetString()!;
                        }
                    }
                    catch (JsonException)
                    {
                    }
                }

                return new BlocketSyncViewModel
                {
                    Limit = model.Limit,
                    OrgId = model.OrgId,
                    Locations = model.Locations,
                    Manufacturers = model.Manufacturers,
                    ErrorMessage = errorMessage
                };
            }

            var result = await response.Content.ReadFromJsonAsync<BlocketSyncViewModel>(JsonOptions);
            if (result is null)
            {
                return new BlocketSyncViewModel
                {
                    Limit = model.Limit,
                    OrgId = model.OrgId,
                    Locations = model.Locations,
                    Manufacturers = model.Manufacturers,
                    ErrorMessage = "The sync completed but no summary was returned."
                };
            }

            result.Limit = model.Limit;
            result.OrgId = model.OrgId;
            result.Locations = model.Locations;
            result.Manufacturers = model.Manufacturers;
            result.HasResult = true;
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unavailable while syncing Blocket vehicles");
            return new BlocketSyncViewModel
            {
                Limit = model.Limit,
                OrgId = model.OrgId,
                Locations = model.Locations,
                Manufacturers = model.Manufacturers,
                ErrorMessage = "API:t kunde inte nås. Upphämtningen från Blocket misslyckades."
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Blocket sync request timed out in web app while API may still be processing");
            return new BlocketSyncViewModel
            {
                Limit = model.Limit,
                OrgId = model.OrgId,
                Locations = model.Locations,
                Manufacturers = model.Manufacturers,
                InfoMessage = "Upphämtningen tar längre tid än väntat men API:t kan fortfarande arbeta i bakgrunden. Vänta en stund och kontrollera fordonslistan igen."
            };
        }
    }

    public async Task<List<BlocketPreviewDto>> PreviewBlocketAsync(BlocketSyncViewModel model)
    {
        try
        {
            (int? minMileage, int? maxMileage) = model.MileageBand switch
            {
                "0-10000"     => (0,     (int?)10000),
                "10000-20000" => (10000, (int?)20000),
                "20000-30000" => (20000, (int?)30000),
                "30000-40000" => (30000, (int?)40000),
                "40000-"      => (40000, (int?)null),
                _             => ((int?)null, (int?)null)
            };

            var response = await _longRunningHttpClient.PostAsJsonAsync($"{_baseUrl}/api/v1/vehicles/preview/blocket", new
            {
                limit = model.Limit,
                query = string.IsNullOrWhiteSpace(model.Query) ? null : model.Query,
                sortOrder = string.IsNullOrWhiteSpace(model.SortOrder) ? null : model.SortOrder,
                orgId = model.OrgId,
                locations = model.Locations,
                manufacturers = model.Manufacturers,
                priceFrom = model.PriceFrom,
                priceTo = model.PriceTo,
                yearFrom = model.YearFrom,
                yearTo = model.YearTo,
                minMileage,
                maxMileage,
                colors = string.IsNullOrWhiteSpace(model.Colors) ? null : model.Colors,
                transmissionFilter = string.IsNullOrWhiteSpace(model.TransmissionFilter) ? null : model.TransmissionFilter,
                wheelDrive = string.IsNullOrWhiteSpace(model.WheelDrive) ? null : model.WheelDrive,
                horsepowerFrom = model.HorsepowerFrom,
                horsepowerTo = model.HorsepowerTo,
                fuelTypeFilter = string.IsNullOrWhiteSpace(model.FuelTypeFilter) ? null : model.FuelTypeFilter
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Blocket preview failed: {StatusCode}", response.StatusCode);
                return [];
            }

            return await response.Content.ReadFromJsonAsync<List<BlocketPreviewDto>>(JsonOptions) ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API unavailable during Blocket preview");
            return [];
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Blocket preview timed out");
            return [];
        }
    }

    public async Task<ImportSelectedResult> ImportSelectedAsync(List<string> externalIds, Dictionary<string, string> imageUrlsById)
    {
        try
        {
            var response = await _longRunningHttpClient.PostAsJsonAsync(
                $"{_baseUrl}/api/v1/vehicles/import/blocket/selected",
                new { externalListingIds = externalIds, imageUrlsById });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Import selected failed: {StatusCode}", response.StatusCode);
                return new ImportSelectedResult { TotalSelected = externalIds.Count, TotalSkipped = externalIds.Count };
            }

            return await response.Content.ReadFromJsonAsync<ImportSelectedResult>(JsonOptions)
                ?? new ImportSelectedResult { TotalSelected = externalIds.Count };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API unavailable during import selected");
            return new ImportSelectedResult { TotalSelected = externalIds.Count, TotalSkipped = externalIds.Count };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Import selected timed out");
            return new ImportSelectedResult { TotalSelected = externalIds.Count, TotalSkipped = externalIds.Count };
        }
    }

    public async Task<HanteraDatabaseViewModel> GetHanteraDatabaseViewModelAsync()
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var byModelTask = _httpClient.GetFromJsonAsync<IEnumerable<VehicleStatsByModelDto>>($"{_baseUrl}/api/v1/vehicles/stats/by-model", JsonOptions);
            var byMileageTask = _httpClient.GetFromJsonAsync<IEnumerable<VehicleStatsByMileageDto>>($"{_baseUrl}/api/v1/vehicles/stats/by-mileage", JsonOptions);
            var summaryTask = _httpClient.GetFromJsonAsync<VehicleStatsSummaryDto>($"{_baseUrl}/api/v1/vehicles/stats/summary", JsonOptions);

            await Task.WhenAll(byModelTask, byMileageTask, summaryTask);

            return new HanteraDatabaseViewModel
            {
                ByModel = await byModelTask ?? [],
                ByMileage = await byMileageTask ?? [],
                Summary = await summaryTask ?? new VehicleStatsSummaryDto(0, 0, 0, 0)
            };
        }, new HanteraDatabaseViewModel(), "loading database stats");
    }

    public async Task<int> BulkDeleteAsync(string? make, string? model, bool? isSold, int? minMileage, int? maxMileage)
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var queryParams = new Dictionary<string, string?>();
            if (make is not null) queryParams["make"] = make;
            if (model is not null) queryParams["model"] = model;
            if (isSold.HasValue) queryParams["isSold"] = isSold.Value.ToString();
            if (minMileage.HasValue) queryParams["minMileage"] = minMileage.Value.ToString();
            if (maxMileage.HasValue) queryParams["maxMileage"] = maxMileage.Value.ToString();

            var url = QueryHelpers.AddQueryString($"{_baseUrl}/api/v1/vehicles/bulk", queryParams);
            var response = await _longRunningHttpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Bulk delete failed: {StatusCode}", response.StatusCode);
                return 0;
            }

            var result = await response.Content.ReadFromJsonAsync<BulkDeleteResponse>(JsonOptions);
            return result?.TotalDeleted ?? 0;
        }, 0, "bulk deleting vehicles");
    }

    public async Task<int> DeleteAllVehiclesAsync()
    {
        return await ExecuteWithApiFallback(async () =>
        {
            var response = await _longRunningHttpClient.DeleteAsync($"{_baseUrl}/api/v1/vehicles/bulk/all");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Delete all failed: {StatusCode}", response.StatusCode);
                return 0;
            }

            var result = await response.Content.ReadFromJsonAsync<BulkDeleteResponse>(JsonOptions);
            return result?.TotalDeleted ?? 0;
        }, 0, "deleting all vehicles");
    }

    private async Task LoadDropdownData(VehicleBaseViewModel viewModel, VehicleDetailsDto vehicleToEdit)
    {
        try
        {
            var manufacturersTask = GetManufacturersAsync();
            var fuelTypesTask = GetFuelTypesAsync();
            var transmissionsTask = GetTransmissionsAsync();

            await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

            var manufacturers = await manufacturersTask ?? new List<NamedObjectDto>();
            var fuelTypes = await fuelTypesTask ?? new List<NamedObjectDto>();
            var transmissionTypes = await transmissionsTask ?? new List<NamedObjectDto>();

            viewModel.Vehicle.ManufacturerId = manufacturers.FirstOrDefault(m => m.Name.Equals(vehicleToEdit.Manufacturer, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
            viewModel.Vehicle.FuelTypeId = fuelTypes.FirstOrDefault(f => f.Name.Equals(vehicleToEdit.FuelType, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
            viewModel.Vehicle.TransmissionTypeId = transmissionTypes.FirstOrDefault(t => t.Name.Equals(vehicleToEdit.TransmissionType, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

            viewModel.Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList();
            viewModel.FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
            viewModel.TransmissionTypes = transmissionTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unavailable while loading vehicle dropdown data");
            viewModel.Manufacturers = new List<SelectListItem>();
            viewModel.FuelTypes = new List<SelectListItem>();
            viewModel.TransmissionTypes = new List<SelectListItem>();
        }
    }

    private async Task LoadDropdownDataSimple(VehicleBaseViewModel viewModel)
    {
        try
        {
            var manufacturersTask = GetManufacturersAsync();
            var fuelTypesTask = GetFuelTypesAsync();
            var transmissionsTask = GetTransmissionsAsync();

            await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

            var manufacturers = await manufacturersTask ?? new List<NamedObjectDto>();
            var fuelTypes = await fuelTypesTask ?? new List<NamedObjectDto>();
            var transmissionTypes = await transmissionsTask ?? new List<NamedObjectDto>();

            viewModel.Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList();
            viewModel.FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
            viewModel.TransmissionTypes = transmissionTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unavailable while loading vehicle dropdown data");
            viewModel.Manufacturers = new List<SelectListItem>();
            viewModel.FuelTypes = new List<SelectListItem>();
            viewModel.TransmissionTypes = new List<SelectListItem>();
        }
    }

    private Task<List<NamedObjectDto>> GetManufacturersAsync() =>
        GetCachedDropdownDataAsync(ManufacturersCacheKey, $"{_baseUrl}/api/v1/manufacturers");

    private Task<List<NamedObjectDto>> GetFuelTypesAsync() =>
        GetCachedDropdownDataAsync(FuelTypesCacheKey, $"{_baseUrl}/api/v1/fueltypes");

    private Task<List<NamedObjectDto>> GetTransmissionsAsync() =>
        GetCachedDropdownDataAsync(TransmissionsCacheKey, $"{_baseUrl}/api/v1/transmissions");

    private record BulkDeleteResponse(int TotalDeleted);
    private record SeedVehiclesResponse(bool Seeded, string Message);

    private async Task<List<NamedObjectDto>> GetCachedDropdownDataAsync(string cacheKey, string requestUri)
    {
        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DropdownCacheDuration;
            return await _httpClient.GetFromJsonAsync<List<NamedObjectDto>>(requestUri, JsonOptions) ?? new List<NamedObjectDto>();
        });

        return result ?? new List<NamedObjectDto>();
    }

    private async Task<T> ExecuteWithApiFallback<T>(Func<Task<T>> action, T fallbackValue, string operation)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API is unavailable while {operation}", operation);
            return fallbackValue;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "API request timed out while {operation}", operation);
            return fallbackValue;
        }
    }
}
