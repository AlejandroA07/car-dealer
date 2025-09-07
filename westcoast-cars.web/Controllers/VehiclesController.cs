using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using westcoast_cars.web.ViewModels.Vehicles;
using System.Text;
using WestcoastCars.Contracts.DTOs;

[Route("Vehicles")]
public class VehiclesController : Controller
{
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IConfiguration config, HttpClient httpClient, ILogger<VehiclesController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = config["ApiBaseUrl"];
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [HttpGet("list")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles/list");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error fetching vehicle list: {response.StatusCode}");
                return View("Errors");
            }

            var json = await response.Content.ReadAsStringAsync();
            var vehicles = JsonSerializer.Deserialize<List<VehicleSummaryDto>>(json, _options);
            return View("Index", vehicles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Index");
            return View("Errors");
        }
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error fetching vehicle {id}: {response.StatusCode}");
                return Content("Oops, something went wrong");
            }

            var json = await response.Content.ReadAsStringAsync();
            var vehicle = JsonSerializer.Deserialize<VehicleDetailsDto>(json, _options);
            return View("Details", vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Details for ID {id}");
            return View("Errors");
        }
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"{_baseUrl}/api/v1/vehicles/{id}", null);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Vehicle {id} deleted successfully");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogError($"Error deleting vehicle {id}: {response.StatusCode}");
            return View("Errors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Delete for ID {id}");
            return View("Errors");
        }
    }

    // GET: /Vehicles/edit/{id}
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            _logger.LogInformation($"Loading edit form for vehicle {id}");

            var vehicleResponse = await _httpClient.GetAsync($"{_baseUrl}/api/v1/vehicles/{id}");
            if (!vehicleResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Error fetching vehicle {id} for editing: {vehicleResponse.StatusCode}");
                return View("Errors");
            }

            var vehicleJson = await vehicleResponse.Content.ReadAsStringAsync();
            var vehicleToEdit = JsonSerializer.Deserialize<VehicleDetailsDto>(vehicleJson, _options);

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

            // Load dropdown data
            await LoadDropdownData(viewModel, vehicleToEdit);
            
            return View("Edit", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Edit GET for ID {id}");
            return View("Errors");
        }
    }

    // POST: /Vehicles/edit/{id}
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Vehicle")] VehicleDto vehicle)
    {
        // Manually set the ID from the URL, as it's not part of the form prefix
        vehicle.Id = id;
        // Re-validate the model after setting the ID
        ModelState.Clear();
        TryValidateModel(vehicle);

        // Create a view model to pass back to the view in case of error
        var vehicleViewModel = new VehicleBaseViewModel { Vehicle = vehicle };

        try
        {
            _logger.LogInformation($"🚗 STARTING UPDATE for vehicle {id}");
            _logger.LogInformation($"Data received: Model={vehicle?.Model}, RegNo={vehicle?.RegistrationNumber}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid ModelState for vehicle {id}");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"- {error.ErrorMessage}");
                }
                await LoadDropdownDataSimple(vehicleViewModel);
                return View("Edit", vehicleViewModel);
            }

            // VehicleUpdateDto is a superset of VehicleDto, so we can map the properties
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

            var jsonPayload = JsonSerializer.Serialize(updateDto, _options);
            _logger.LogInformation($"JSON sent to API: {jsonPayload}");

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/v1/vehicles/{id}", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"API Response - Status: {response.StatusCode}, Content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"✅ Vehicle {id} updated successfully");
                TempData["SuccessMessage"] = "Vehicle updated successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogError($"❌ Error updating vehicle {id}: {response.StatusCode} - {responseContent}");
                ModelState.AddModelError("", $"Error updating: {response.StatusCode}");
                await LoadDropdownDataSimple(vehicleViewModel);
                return View("Edit", vehicleViewModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"💥 EXCEPTION in Edit POST for ID {id}");
            ModelState.AddModelError("", "An unexpected error occurred");
            await LoadDropdownDataSimple(vehicleViewModel);
            return View("Edit", vehicleViewModel);
        }
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var viewModel = new VehicleBaseViewModel
            {
                Vehicle = new VehicleDto() // Initialize with empty object
            };
            await LoadDropdownDataSimple(viewModel);
            return View("Create", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Create GET");
            return View("Errors");
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(VehicleBaseViewModel vehicleViewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownDataSimple(vehicleViewModel);
                return View("Create", vehicleViewModel);
            }

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

            var jsonPayload = JsonSerializer.Serialize(postDto, _options);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/vehicles", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Vehicle created successfully");
                return RedirectToAction(nameof(Index));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Error creating vehicle: {response.StatusCode} - {errorContent}");
            ModelState.AddModelError(string.Empty, "Error creating vehicle");

            await LoadDropdownDataSimple(vehicleViewModel);
            return View("Create", vehicleViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Create POST");
            await LoadDropdownDataSimple(vehicleViewModel);
            return View("Create", vehicleViewModel);
        }
    }

    // 🔧 HELPER METHOD - Load dropdowns with vehicle context
    private async Task LoadDropdownData(VehicleBaseViewModel viewModel, VehicleDetailsDto vehicleToEdit)
    {
        try
        {
            var manufacturersTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/manufacturers");
            var fuelTypesTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/fueltypes");
            var transmissionsTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/transmissionTypes");
            
            await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

            var manufacturers = JsonSerializer.Deserialize<List<NamedObjectDto>>(await manufacturersTask, _options);
            var fuelTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await fuelTypesTask, _options);
            var transmissionsTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await transmissionsTask, _options);

            // Assign correct IDs based on the current vehicle's names
            viewModel.Vehicle.ManufacturerId = manufacturers.FirstOrDefault(m => 
                m.Name.Equals(vehicleToEdit.Manufacturer, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
            viewModel.Vehicle.FuelTypeId = fuelTypes.FirstOrDefault(f => 
                f.Name.Equals(vehicleToEdit.FuelType, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
            viewModel.Vehicle.TransmissionTypeId = transmissionsTypes.FirstOrDefault(t => 
                t.Name.Equals(vehicleToEdit.TransmissionsType, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

            viewModel.Manufacturers = manufacturers.Select(m => new SelectListItem 
                { Value = m.Id.ToString(), Text = m.Name }).ToList();
            viewModel.FuelTypes = fuelTypes.Select(f => new SelectListItem 
                { Value = f.Id.ToString(), Text = f.Name }).ToList();
            viewModel.TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem 
                { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dropdowns with context");
        }
    }

    // 🔧 HELPER METHOD - Load lists without assigning IDs
    private async Task LoadDropdownDataSimple(VehicleBaseViewModel viewModel)
    {
        try
        {
            var manufacturersTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/manufacturers");
            var fuelTypesTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/fueltypes");
            var transmissionsTask = _httpClient.GetStringAsync($"{_baseUrl}/api/v1/transmissionTypes");
            
            await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

            var manufacturers = JsonSerializer.Deserialize<List<NamedObjectDto>>(await manufacturersTask, _options);
            var fuelTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await fuelTypesTask, _options);
            var transmissionsTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await transmissionsTask, _options);

            viewModel.Manufacturers = manufacturers.Select(m => new SelectListItem 
                { Value = m.Id.ToString(), Text = m.Name }).ToList();
            viewModel.FuelTypes = fuelTypes.Select(f => new SelectListItem 
                { Value = f.Id.ToString(), Text = f.Name }).ToList();
            viewModel.TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem 
                { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading simple dropdowns");
        }
    }
}
