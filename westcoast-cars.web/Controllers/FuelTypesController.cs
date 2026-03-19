using Microsoft.AspNetCore.Mvc;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.FuelType;

namespace westcoast_cars.web.Controllers
{
    [Route("FuelTypes")]
    public class FuelTypesController : Controller
    {
        private readonly IFuelTypeService _fuelTypeService;
        private readonly ILogger<FuelTypesController> _logger;

        public FuelTypesController(IFuelTypeService fuelTypeService, ILogger<FuelTypesController> logger)
        {
            _fuelTypeService = fuelTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var fuelTypes = await _fuelTypeService.ListAllAsync();
                return View("Index", fuelTypes);
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
            var fuelTypes = await _fuelTypeService.ListAllAsync();
            var model = new FuelTypePostViewModel
            {
                FuelTypes = fuelTypes
            };
            return View("Create", model);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(FuelTypePostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.FuelTypes = await _fuelTypeService.ListAllAsync();
                    return View(model);
                }

                var result = await _fuelTypeService.CreateAsync(model);

                if (result)
                {
                    TempData["success"] = "Fuel type created successfully";
                    return RedirectToAction(nameof(Create));
                }

                TempData["error"] = "API Error: Could not create fuel type";
                model.FuelTypes = await _fuelTypeService.ListAllAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create POST");
                TempData["error"] = "An unexpected error occurred";
                return View("Errors");
            }
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _fuelTypeService.DeleteAsync(id);
                if (result)
                {
                    TempData["success"] = "Fuel type deleted successfully";
                    return RedirectToAction(nameof(Create));
                }
                TempData["error"] = "Could not delete fuel type";
                return View("Errors");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                TempData["error"] = "An unexpected error occurred";
                return View("Errors");
            }
        }
    }
}
