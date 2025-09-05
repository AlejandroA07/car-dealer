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
    private readonly IHttpClientFactory _httpClient;

    public VehiclesController(IConfiguration config, IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = config["ApiBaseUrl"];
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [HttpGet("list")]
    public async Task<IActionResult> Index()
    {
        using var client = _httpClient.CreateClient();
        var response = await client.GetAsync($"{_baseUrl}/api/v1/vehicles/list");

        if (!response.IsSuccessStatusCode)
        {
            return View("Errors");
        }

        var json = await response.Content.ReadAsStringAsync();
        var vehicles = JsonSerializer.Deserialize<List<VehicleSummaryDto>>(json, _options);
        return View("Index", vehicles);
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        using var client = _httpClient.CreateClient();
        var response = await client.GetAsync($"{_baseUrl}/api/v1/vehicles/{id}");
        if (!response.IsSuccessStatusCode) return Content("Ops, det gick fel");
        var json = await response.Content.ReadAsStringAsync();
        var vehicle = JsonSerializer.Deserialize<VehicleDetailsDto>(json, _options);
        return View("Details", vehicle);
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        using var client = _httpClient.CreateClient();
        var response = await client.PatchAsync($"{_baseUrl}/api/v1/vehicles/{id}", null);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }
        return View("Errors");
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var client = _httpClient.CreateClient();
        var vehicleResponse = await client.GetAsync($"{_baseUrl}/api/v1/vehicles/{id}");
        if (!vehicleResponse.IsSuccessStatusCode) return View("Errors");
        var vehicleJson = await vehicleResponse.Content.ReadAsStringAsync();
        var vehicleToEdit = JsonSerializer.Deserialize<VehicleDetailsDto>(vehicleJson, _options);

        var manufacturersTask = client.GetStringAsync($"{_baseUrl}/api/v1/manufacturers");
        var fuelTypesTask = client.GetStringAsync($"{_baseUrl}/api/v1/fueltypes");
        var transmissionsTask = client.GetStringAsync($"{_baseUrl}/api/v1/transmissionTypes");
        await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

        var manufacturers = JsonSerializer.Deserialize<List<NamedObjectDto>>(await manufacturersTask, _options);
        var fuelTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await fuelTypesTask, _options);
        var transmissionsTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await transmissionsTask, _options);

        var viewModel = new VehicleBaseViewModel
        {
            Vehicle = new VehicleDto
            {
                Id = vehicleToEdit.Id, // Corrected: Id is now in VehicleDto
                Model = vehicleToEdit.Model,
                ModelYear = vehicleToEdit.ModelYear,
                Mileage = vehicleToEdit.Mileage,
                Value = vehicleToEdit.Value,
                Description = vehicleToEdit.Description,
                ManufacturerId = manufacturers.FirstOrDefault(m => m.Name == vehicleToEdit.Manufacturer)?.Id ?? 0, // Corrected: Get from manufacturers list
                FuelTypeId = fuelTypes.FirstOrDefault(f => f.Name == vehicleToEdit.FuelType)?.Id ?? 0, // Corrected: Get from fuelTypes list
                TransmissionTypeId = transmissionsTypes.FirstOrDefault(t => t.Name == vehicleToEdit.TransmissionsType)?.Id ?? 0, // Corrected: Get from transmissionsTypes list
            },
            Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList(),
            FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList(),
            TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList()
        };
        return View("Edit", viewModel);
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(int id, VehicleBaseViewModel vehicleViewModel)
    {
        if (!ModelState.IsValid) return View(vehicleViewModel);

        using var client = _httpClient.CreateClient();

        var updateDto = new VehicleUpdateDto
        {
            Id = id,
            Model = vehicleViewModel.Vehicle.Model,
            ModelYear = vehicleViewModel.Vehicle.ModelYear,
            Mileage = vehicleViewModel.Vehicle.Mileage,
            Description = vehicleViewModel.Vehicle.Description,
            Value = vehicleViewModel.Vehicle.Value,
            IsSold = vehicleViewModel.Vehicle.IsSold,
            ImageUrl = vehicleViewModel.Vehicle.ImageUrl,
            ManufacturerId = vehicleViewModel.Vehicle.ManufacturerId, // Corrected line
            FuelTypeId = vehicleViewModel.Vehicle.FuelTypeId, // Corrected line
            TransmissionTypeId = vehicleViewModel.Vehicle.TransmissionTypeId // Corrected line
        };

        var jsonPayload = JsonSerializer.Serialize(updateDto);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"{_baseUrl}/api/v1/vehicles/{id}", content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        // Handle error, maybe repopulate dropdowns and return view
        return View("Errors");
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        using var client = _httpClient.CreateClient();
        var manufacturersTask = client.GetStringAsync($"{_baseUrl}/api/v1/manufacturers");
        var fuelTypesTask = client.GetStringAsync($"{_baseUrl}/api/v1/fueltypes");
        var transmissionsTask = client.GetStringAsync($"{_baseUrl}/api/v1/transmissionTypes");
        await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

        var manufacturers = JsonSerializer.Deserialize<List<NamedObjectDto>>(await manufacturersTask, _options);
        var fuelTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await fuelTypesTask, _options);
        var transmissionsTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await transmissionsTask, _options);

        var viewModel = new VehicleBaseViewModel
        {
            Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList(),
            FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList(),
            TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList()
        };
        return View("Create", viewModel);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(VehicleBaseViewModel vehicleViewModel)
    {
        if (!ModelState.IsValid) 
        {
            // If model state is invalid, we need to reload the dropdowns before returning the view.
            using var client = _httpClient.CreateClient();
            var manufacturersTask = client.GetStringAsync($"{_baseUrl}/api/v1/manufacturers");
            var fuelTypesTask = client.GetStringAsync($"{_baseUrl}/api/v1/fueltypes");
            var transmissionsTask = client.GetStringAsync($"{_baseUrl}/api/v1/transmissionTypes");
            await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

            var manufacturers = JsonSerializer.Deserialize<List<NamedObjectDto>>(await manufacturersTask, _options);
            var fuelTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await fuelTypesTask, _options);
            var transmissionsTypes = JsonSerializer.Deserialize<List<NamedObjectDto>>(await transmissionsTask, _options);

            vehicleViewModel.Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList();
            vehicleViewModel.FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
            vehicleViewModel.TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
            
            return View("Create", vehicleViewModel);
        }

        using (var client = _httpClient.CreateClient())
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

            var jsonPayload = JsonSerializer.Serialize(postDto);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_baseUrl}/api/v1/vehicles", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "An error occurred while creating the vehicle via the API.");
            return View("Create", vehicleViewModel);
        }
    }
}