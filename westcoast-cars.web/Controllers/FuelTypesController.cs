using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using westcoast_cars.web.ViewModels.FuelType;
using System.Collections.Generic;

namespace westcoast_cars.web.Controllers
{
    [Route("FuelTypes")]
    public class FuelTypesController : Controller
    {
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _options;
        private readonly IHttpClientFactory _httpClient;

        public FuelTypesController(IConfiguration config, IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiBaseUrl"];
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IActionResult> Index()
        {
            using var client = _httpClient.CreateClient();
            var response = await client.GetAsync($"{_baseUrl}/api/v1/fueltypes");

            if (!response.IsSuccessStatusCode)
            {
                return View("Errors");
            }

            var json = await response.Content.ReadAsStringAsync();
            var fuelTypes = JsonSerializer.Deserialize<List<FuelTypeListViewModel>>(json, _options);
            return View("Index", fuelTypes);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var fuelTypes = await GetFuelTypesForDropdown();

            var model = new FuelTypePostViewModel
            {
                FuelTypes = fuelTypes
            };

            return View("Create", model);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(FuelTypePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.FuelTypes = await GetFuelTypesForDropdown();
                return View(model);
            }

            using var client = _httpClient.CreateClient();

            var apiPayload = new { Name = model.Name };
            var jsonPayload = JsonSerializer.Serialize(apiPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_baseUrl}/api/v1/fueltypes", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Create));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {errorContent}");
            model.FuelTypes = await GetFuelTypesForDropdown();
            return View(model);
        }

        private async Task<IList<FuelTypeListViewModel>> GetFuelTypesForDropdown()
        {
            using var client = _httpClient.CreateClient();
            var response = await client.GetAsync($"{_baseUrl}/api/v1/fueltypes");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var fuelTypes = JsonSerializer.Deserialize<List<FuelTypeListViewModel>>(json, _options);
            return fuelTypes;
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var client = _httpClient.CreateClient();
            var response = await client.DeleteAsync($"{_baseUrl}/api/v1/fueltypes/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Create));
            }

            return View("Errors");
        }
    }
}