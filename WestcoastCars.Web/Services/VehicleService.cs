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
                Value = vehicleToEdit.Value,
                Description = vehicleToEdit.Description,
                IsSold = vehicleToEdit.IsSold,
                ImageUrl = vehicleToEdit.ImageUrl
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
            Value = vehicle.Value,
            IsSold = vehicle.IsSold,
            ImageUrl = vehicle.ImageUrl,
            ManufacturerId = vehicle.ManufacturerId,
            FuelTypeId = vehicle.FuelTypeId,
            TransmissionTypeId = vehicle.TransmissionTypeId,
            RegistrationNumber = vehicle.RegistrationNumber
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
            Value = vehicleViewModel.Vehicle.Value,
            Description = vehicleViewModel.Vehicle.Description,
            IsSold = vehicleViewModel.Vehicle.IsSold,
            ImageUrl = vehicleViewModel.Vehicle.ImageUrl
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
            var response = await _longRunningHttpClient.PostAsJsonAsync($"{_baseUrl}/api/v1/vehicles/import/blocket", new
            {
                limit = model.Limit,
                orgId = model.OrgId,
                locations = model.Locations,
                models = model.Models
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
                    Models = model.Models,
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
                    Models = model.Models,
                    ErrorMessage = "The sync completed but no summary was returned."
                };
            }

            result.Limit = model.Limit;
            result.OrgId = model.OrgId;
            result.Locations = model.Locations;
            result.Models = model.Models;
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
                Models = model.Models,
                ErrorMessage = "API:t kunde inte nås för Blocket-synkningen."
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
                Models = model.Models,
                InfoMessage = "Synkningen tar längre tid än väntat men API:t kan fortfarande arbeta i bakgrunden. Vänta en stund och kontrollera fordonslistan igen."
            };
        }
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
