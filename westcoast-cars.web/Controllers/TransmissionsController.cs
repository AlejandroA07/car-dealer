using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using westcoast_cars.web.ViewModels.TransmissionType;
using System.Collections.Generic;

namespace westcoast_cars.web.Controllers
{
    [Route("Transmissions")]
    public class TransmissionsController : Controller
    {
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _options;
        private readonly IHttpClientFactory _httpClient;

        public TransmissionsController(IConfiguration config, IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiBaseUrl"];
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IActionResult> Index()
        {
            using var client = _httpClient.CreateClient();
            var response = await client.GetAsync($"{_baseUrl}/api/v1/transmissionTypes");

            if (!response.IsSuccessStatusCode)
            {
                return View("Errors");
            }

            var json = await response.Content.ReadAsStringAsync();
            var transmissionTypes = JsonSerializer.Deserialize<List<TransmissionTypeListViewModel>>(json, _options);
            return View("Index", transmissionTypes);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var transmissionTypes = await GetTransmissionTypesForDropdown();

            var model = new TransmissionTypePostViewModel
            {
                TransmissionTypes = transmissionTypes
            };

            return View("Create", model);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(TransmissionTypePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TransmissionTypes = await GetTransmissionTypesForDropdown();
                return View(model);
            }

            using var client = _httpClient.CreateClient();

            var apiPayload = new { Name = model.Name };
            var jsonPayload = JsonSerializer.Serialize(apiPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_baseUrl}/api/v1/transmissionTypes", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Create));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {errorContent}");
            model.TransmissionTypes = await GetTransmissionTypesForDropdown();
            return View(model);
        }

        private async Task<IList<TransmissionTypeListViewModel>> GetTransmissionTypesForDropdown()
        {
            using var client = _httpClient.CreateClient();
            var response = await client.GetAsync($"{_baseUrl}/api/v1/transmissionTypes");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var transmissionTypes = JsonSerializer.Deserialize<List<TransmissionTypeListViewModel>>(json, _options);
            return transmissionTypes;
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var client = _httpClient.CreateClient();
            var response = await client.DeleteAsync($"{_baseUrl}/api/v1/transmissionTypes/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Create));
            }

            return View("Errors");
        }
    }
}