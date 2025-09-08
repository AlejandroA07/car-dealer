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
                var transmissionTypes = await _transmissionTypeService.ListAllTransmissionTypesAsync();
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
            var transmissionTypes = await _transmissionTypeService.ListAllTransmissionTypesAsync();
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
                    model.TransmissionTypes = await _transmissionTypeService.ListAllTransmissionTypesAsync();
                    return View(model);
                }

                var result = await _transmissionTypeService.CreateTransmissionTypeAsync(model);

                if (result)
                {
                    return RedirectToAction(nameof(Create));
                }

                ModelState.AddModelError(string.Empty, "API Error: Could not create transmission type");
                model.TransmissionTypes = await _transmissionTypeService.ListAllTransmissionTypesAsync();
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
                var result = await _transmissionTypeService.DeleteTransmissionTypeAsync(id);
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
