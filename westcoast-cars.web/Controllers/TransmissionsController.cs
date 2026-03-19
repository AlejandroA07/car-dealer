using Microsoft.AspNetCore.Mvc;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.TransmissionType;

namespace westcoast_cars.web.Controllers
{
    [Route("Transmissions")]
    public class TransmissionsController : Controller
    {
        private readonly ITransmissionTypeService _transmissionTypeService;
        private readonly ILogger<TransmissionsController> _logger;

        public TransmissionsController(ITransmissionTypeService transmissionTypeService, ILogger<TransmissionsController> logger)
        {
            _transmissionTypeService = transmissionTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var transmissionTypes = await _transmissionTypeService.ListAllAsync();
                return View("Index", transmissionTypes);
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
            var transmissionTypes = await _transmissionTypeService.ListAllAsync();
            var model = new TransmissionTypePostViewModel
            {
                TransmissionTypes = transmissionTypes
            };
            return View("Create", model);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(TransmissionTypePostViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.TransmissionTypes = await _transmissionTypeService.ListAllAsync();
                    return View(model);
                }

                var result = await _transmissionTypeService.CreateAsync(model);

                if (result)
                {
                    TempData["success"] = "Transmission type created successfully";
                    return RedirectToAction(nameof(Create));
                }

                TempData["error"] = "API Error: Could not create transmission type";
                model.TransmissionTypes = await _transmissionTypeService.ListAllAsync();
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
                var result = await _transmissionTypeService.DeleteAsync(id);
                if (result)
                {
                    TempData["success"] = "Transmission type deleted successfully";
                    return RedirectToAction(nameof(Create));
                }
                TempData["error"] = "Could not delete transmission type";
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
