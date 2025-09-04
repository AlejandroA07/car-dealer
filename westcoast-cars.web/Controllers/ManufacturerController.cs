using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectListItem
using westcoast_cars.web.ViewModels.Manufacturer;

namespace westcoast_cars.web.Controllers
{
    [Route("Manufacturer")] // Changed from [controller] to explicit "Manufacturer"
    public class ManufacturerController : Controller
    {
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _options;
        private readonly IHttpClientFactory _httpClient;

        // A private helper class for deserializing lists from the API for dropdowns
        private class SelectListItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public ManufacturerController(IConfiguration config, IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiBaseUrl"];
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // This Index action will now list all manufacturers
        public async Task<IActionResult> Index()
        {
            using var client = _httpClient.CreateClient();
            var response = await client.GetAsync($"{_baseUrl}/api/v1/manufacturers");

            if (!response.IsSuccessStatusCode)
            {
                return View("Errors"); // Assuming an Errors view exists
            }

            var json = await response.Content.ReadAsStringAsync();
            var manufacturers = JsonSerializer.Deserialize<List<ManufacturerListViewModel>>(json, _options);
            return View("Index", manufacturers);
        }

        [HttpGet("Create")] // Explicit route for clarity
        public async Task<IActionResult> Create()
        {
            var manufacturers = await GetManufacturersForDropdown();

            var model = new ManufacturerPostViewModel
            {
                Manufacturers = manufacturers
            };

            return View("Create", model);
        }

        [HttpPost("Create")] // Explicit route for clarity
        public async Task<IActionResult> Create(ManufacturerPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Manufacturers = await GetManufacturersForDropdown();
                return View(model);
            }

            using var client = _httpClient.CreateClient();

            // The API expects a PostViewModel with a 'Name' property
            var apiPayload = new { Name = model.Name };
            var jsonPayload = JsonSerializer.Serialize(apiPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_baseUrl}/api/v1/manufacturers", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Create));
            }

            // Handle API errors
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {errorContent}");
            model.Manufacturers = await GetManufacturersForDropdown();
            return View(model);
        }

        // Helper method to fetch manufacturers for dropdowns
        private async Task<IList<ManufacturerListViewModel>> GetManufacturersForDropdown()
        {
            using var client = _httpClient.CreateClient();
            var response = await client.GetAsync($"{_baseUrl}/api/v1/manufacturers");
            response.EnsureSuccessStatusCode(); // Throw if not success

            var json = await response.Content.ReadAsStringAsync();
            var manufacturers = JsonSerializer.Deserialize<List<ManufacturerListViewModel>>(json, _options);
            return manufacturers;
        }
    }
}