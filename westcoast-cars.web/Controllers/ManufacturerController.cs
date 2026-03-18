using Microsoft.AspNetCore.Mvc;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.Manufacturer;

namespace westcoast_cars.web.Controllers
{
    [Route("Manufacturer")]
    public class ManufacturerController : Controller
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly ILogger<ManufacturerController> _logger;

        public ManufacturerController(IManufacturerService manufacturerService, ILogger<ManufacturerController> logger)
        {
            _manufacturerService = manufacturerService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var manufacturers = await _manufacturerService.ListAllAsync();
                return View("Index", manufacturers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index");
                return View("Errors");
            }
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var manufacturers = await _manufacturerService.ListAllAsync();
            var model = new ManufacturerPostViewModel
            {
                Manufacturers = manufacturers
            };
            return View("Create", model);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(ManufacturerPostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Manufacturers = await _manufacturerService.ListAllAsync();
                    return View(model);
                }

                var result = await _manufacturerService.CreateAsync(model);

                if (result)
                {
                    return RedirectToAction(nameof(Create));
                }

                ModelState.AddModelError(string.Empty, "API Error: Could not create manufacturer");
                model.Manufacturers = await _manufacturerService.ListAllAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create POST");
                return View("Errors");
            }
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _manufacturerService.DeleteAsync(id);
                if (result)
                {
                    return RedirectToAction(nameof(Create));
                }
                return View("Errors");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return View("Errors");
            }
        }
    }
}
