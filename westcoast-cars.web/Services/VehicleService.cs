using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using westcoast_cars.web.ViewModels.Vehicles;
using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VehicleService> _logger;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _options;

        public VehicleService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<VehicleService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _baseUrl = config["Services:ApiUrl"] ?? throw new InvalidOperationException("Services:ApiUrl is not configured");
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<VehicleSummaryDto>> ListVehiclesAsync()
        {
            return await ExecuteWithApiFallback(async () =>
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles/list");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error fetching vehicle list: {response.StatusCode}");
                    return new List<VehicleSummaryDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<VehicleSummaryDto>>(json, _options) ?? new List<VehicleSummaryDto>();
            }, new List<VehicleSummaryDto>(), "listing vehicles");
        }

        public async Task<List<VehicleSummaryDto>> ListAllVehiclesAsync()
        {
            return await ExecuteWithApiFallback(async () =>
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles/list-all");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error fetching all vehicles list: {response.StatusCode}");
                    return new List<VehicleSummaryDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<VehicleSummaryDto>>(json, _options) ?? new List<VehicleSummaryDto>();
            }, new List<VehicleSummaryDto>(), "listing all vehicles");
        }

        public async Task<VehicleDetailsDto?> GetVehicleByIdAsync(int id)
        {
            return await ExecuteWithApiFallback(async () =>
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error fetching vehicle {id}: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<VehicleDetailsDto>(json, _options);
            }, null, $"getting vehicle {id}");
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            return await ExecuteWithApiFallback(async () =>
            {
                var response = await _httpClient.PatchAsync($"{_baseUrl}/api/v1/vehicles/{id}", null);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Vehicle {id} deleted successfully");
                    return true;
                }

                _logger.LogError($"Error deleting vehicle {id}: {response.StatusCode}");
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
                    RegistrationNumber = vehicleToEdit.RegistrationNumber,
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
                var jsonPayload = JsonSerializer.Serialize(updateDto, _options);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/api/v1/vehicles/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Vehicle {id} updated successfully");
                    return true;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error updating vehicle {id}: {response.StatusCode} - {responseContent}");
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
                var jsonPayload = JsonSerializer.Serialize(postDto, _options);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/vehicles", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Vehicle created successfully");
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error creating vehicle: {response.StatusCode} - {errorContent}");
                return false;
            }, false, "creating vehicle");
        }

        public async Task<List<VehicleSummaryDto>> SearchVehiclesAsync(VehicleSearchDto search)
    {
        var queryParams = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(search.Make)) queryParams["Make"] = search.Make;
        if (!string.IsNullOrWhiteSpace(search.Model)) queryParams["Model"] = search.Model;
        if (search.MinYear.HasValue) queryParams["MinYear"] = search.MinYear.Value.ToString();
        if (search.MaxYear.HasValue) queryParams["MaxYear"] = search.MaxYear.Value.ToString();
        if (search.MinPrice.HasValue) queryParams["MinPrice"] = search.MinPrice.Value.ToString();
        if (search.MaxPrice.HasValue) queryParams["MaxPrice"] = search.MaxPrice.Value.ToString();
        if (search.IsSold.HasValue) queryParams["IsSold"] = search.IsSold.Value.ToString();

        return await ExecuteWithApiFallback(async () =>
        {
            var url = QueryHelpers.AddQueryString($"{_baseUrl}/api/v1/vehicles/search", queryParams);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error searching vehicles: {response.StatusCode}");
                return new List<VehicleSummaryDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<VehicleSummaryDto>>(json, _options) ?? new List<VehicleSummaryDto>();
        }, new List<VehicleSummaryDto>(), "searching vehicles");
    }

    private async Task LoadDropdownData(VehicleBaseViewModel viewModel, VehicleDetailsDto vehicleToEdit)
        {
            try
            {
                var manufacturersTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/manufacturers");
                var fuelTypesTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/fueltypes");
                var transmissionsTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/transmissions");
                
                await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

                var manufacturers = JsonSerializer.Deserialize<List<NamedObjectDto>>(await manufacturersTask, _options) ?? new List<NamedObjectDto>();
                var fuelTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await fuelTypesTask, _options) ?? new List<NamedObjectDto>();
                var transmissionsTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await transmissionsTask, _options) ?? new List<NamedObjectDto>();

                viewModel.Vehicle.ManufacturerId = manufacturers.FirstOrDefault(m => m.Name.Equals(vehicleToEdit.Manufacturer, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
                viewModel.Vehicle.FuelTypeId = fuelTypes.FirstOrDefault(f => f.Name.Equals(vehicleToEdit.FuelType, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
                viewModel.Vehicle.TransmissionTypeId = transmissionsTypes.FirstOrDefault(t => t.Name.Equals(vehicleToEdit.TransmissionsType, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

                viewModel.Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList();
                viewModel.FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
                viewModel.TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API is unavailable while loading vehicle dropdown data");
                viewModel.Manufacturers = new List<SelectListItem>();
                viewModel.FuelTypes = new List<SelectListItem>();
                viewModel.TransmissionsTypes = new List<SelectListItem>();
            }
        }

        private async Task LoadDropdownDataSimple(VehicleBaseViewModel viewModel)
        {
            try
            {
                var manufacturersTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/manufacturers");
                var fuelTypesTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/fueltypes");
                var transmissionsTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/transmissions");
                
                await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

                var manufacturers = JsonSerializer.Deserialize<List<NamedObjectDto>>(await manufacturersTask, _options) ?? new List<NamedObjectDto>();
                var fuelTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await fuelTypesTask, _options) ?? new List<NamedObjectDto>();
                var transmissionsTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await transmissionsTask, _options) ?? new List<NamedObjectDto>();

                viewModel.Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList();
                viewModel.FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
                viewModel.TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API is unavailable while loading vehicle dropdown data");
                viewModel.Manufacturers = new List<SelectListItem>();
                viewModel.FuelTypes = new List<SelectListItem>();
                viewModel.TransmissionsTypes = new List<SelectListItem>();
            }
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
}
