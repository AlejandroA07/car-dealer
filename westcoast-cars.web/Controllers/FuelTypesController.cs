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
                var fuelTypes = await _fuelTypeService.ListAllFuelTypesAsync();
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
            var fuelTypes = await _fuelTypeService.ListAllFuelTypesAsync();
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
                    model.FuelTypes = await _fuelTypeService.ListAllFuelTypesAsync();
                    return View(model);
                }

                var result = await _fuelTypeService.CreateFuelTypeAsync(model);

                if (result)
                {
                    return RedirectToAction(nameof(Create));
                }

                ModelState.AddModelError(string.Empty, "API Error: Could not create fuel type");
                model.FuelTypes = await _fuelTypeService.ListAllFuelTypesAsync();
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
                var result = await _fuelTypeService.DeleteFuelTypeAsync(id);
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
