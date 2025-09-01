using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using westcoast_cars.web.ViewModels.Vehicles;
using System.Text;

[Route("Vehicles")]
public class VehiclesController : Controller
{
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _options;
    private readonly IHttpClientFactory _httpClient;

    // A private helper class for deserializing lists from the API
    private class SelectListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public VehiclesController(IConfiguration config, IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = config.GetSection("apiSettings")["baseUrl"];
        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [HttpGet("list")]
    public async Task<IActionResult> Index()
    {
        using var client = _httpClient.CreateClient();
        var response = await client.GetAsync($"{_baseUrl}/vehicles");
        if (!response.IsSuccessStatusCode) return Content("Ops, det gick fel");
        var json = await response.Content.ReadAsStringAsync();
        var vehicles = JsonSerializer.Deserialize<List<VehicleListViewModel>>(json, _options);
        return View("Index", vehicles);
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        using var client = _httpClient.CreateClient();
        var response = await client.GetAsync($"{_baseUrl}/vehicles/{id}");
        if (!response.IsSuccessStatusCode) return Content("Ops, det gick fel");
        var json = await response.Content.ReadAsStringAsync();
        var vehicle = JsonSerializer.Deserialize<VehicleDetailsViewModel>(json, _options);
        return View("Details", vehicle);
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        using var client = _httpClient.CreateClient();
        var response = await client.PatchAsync($"{_baseUrl}/vehicles/{id}", null);
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
        var vehicleResponse = await client.GetAsync($"{_baseUrl}/vehicles/{id}");
        if (!vehicleResponse.IsSuccessStatusCode) return View("Errors");
        var vehicleJson = await vehicleResponse.Content.ReadAsStringAsync();
        var vehicleToEdit = JsonSerializer.Deserialize<VehicleDetailsViewModel>(vehicleJson, _options);

        var manufacturersTask = client.GetStringAsync($"{_baseUrl}/manufacturers");
        var fuelTypesTask = client.GetStringAsync($"{_baseUrl}/fueltypes");
        var transmissionsTask = client.GetStringAsync($"{_baseUrl}/transmissiontypes");
        await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

        var manufacturers = JsonSerializer.Deserialize<List<SelectListItemDto>>(await manufacturersTask, _options);
        var fuelTypes = JsonSerializer.Deserialize<List<SelectListItemDto>>(await fuelTypesTask, _options);
        var transmissionsTypes = JsonSerializer.Deserialize<List<SelectListItemDto>>(await transmissionsTask, _options);

        var viewModel = new VehicleEditViewModel
        {
            Id = vehicleToEdit.Id,
            Model = vehicleToEdit.Model,
            ModelYear = vehicleToEdit.ModelYear,
            Mileage = vehicleToEdit.Mileage,
            Value = vehicleToEdit.Value,
            Description = vehicleToEdit.Description,
            // Find the ID of the current selection to pre-select the dropdown
            Manufacturer = manufacturers.FirstOrDefault(m => m.Name == vehicleToEdit.Manufacturer)?.Id ?? 0,
            FuelType = fuelTypes.FirstOrDefault(f => f.Name == vehicleToEdit.FuelType)?.Id ?? 0,
            TransmissionsType = transmissionsTypes.FirstOrDefault(t => t.Name == vehicleToEdit.TransmissionsType)?.Id ?? 0,
            Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList(),
            FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList(),
            TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList()
        };
        return View("Edit", viewModel);
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(int id, VehicleEditViewModel vehicle)
    {
        if (!ModelState.IsValid) return View(vehicle);
        using var client = _httpClient.CreateClient();
        var updatePayload = new
        {
            Id = id,
            Model = vehicle.Model,
            ModelYear = vehicle.ModelYear,
            Mileage = vehicle.Mileage,
            Description = vehicle.Description,
            Value = vehicle.Value,
            IsSold = false,
            ImageUrl = "",
            ManufacturerId = vehicle.Manufacturer,
            FuelTypeId = vehicle.FuelType,
            TransmissionTypeId = vehicle.TransmissionsType
        };
        var jsonPayload = JsonSerializer.Serialize(updatePayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"{_baseUrl}/vehicles/{id}", content);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }
        return View("Errors");
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        using var client = _httpClient.CreateClient();
        var manufacturersTask = client.GetStringAsync($"{_baseUrl}/manufacturers");
        var fuelTypesTask = client.GetStringAsync($"{_baseUrl}/fueltypes");
        var transmissionsTask = client.GetStringAsync($"{_baseUrl}/transmissiontypes");
        await Task.WhenAll(manufacturersTask, fuelTypesTask, transmissionsTask);

        var manufacturers = JsonSerializer.Deserialize<List<SelectListItemDto>>(await manufacturersTask, _options);
        var fuelTypes = JsonSerializer.Deserialize<List<SelectListItemDto>>(await fuelTypesTask, _options);
        var transmissionsTypes = JsonSerializer.Deserialize<List<SelectListItemDto>>(await transmissionsTask, _options);

        var viewModel = new VehiclePostViewModel
        {
            Manufacturers = manufacturers.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }).ToList(),
            FuelTypes = fuelTypes.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList(),
            TransmissionsTypes = transmissionsTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList()
        };
        return View("Create", viewModel);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(VehiclePostViewModel vehicle)
    {
        if (!ModelState.IsValid) return View("Create", vehicle);
        using var client = _httpClient.CreateClient();
        var jsonPayload = JsonSerializer.Serialize(vehicle);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{_baseUrl}/vehicles", content);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, "An error occurred while creating the vehicle via the API.");
        return View("Create", vehicle);
    }
}
