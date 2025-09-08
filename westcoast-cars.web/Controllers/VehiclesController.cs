using Microsoft.AspNetCore.Mvc;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.Vehicles;
using WestcoastCars.Contracts.DTOs;

[Route("Vehicles")]
public class VehiclesController : Controller
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var vehicles = await _vehicleService.ListVehiclesAsync();
            return View("Index", vehicles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Index");
            return View("Errors");
        }
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle is null)
            {
                return NotFound();
            }
            return View("Details", vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Details for ID {id}");
            return View("Errors");
        }
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _vehicleService.DeleteVehicleAsync(id);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
            return View("Errors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Delete for ID {id}");
            return View("Errors");
        }
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var viewModel = await _vehicleService.GetVehicleForEditAsync(id);
            if (viewModel is null)
            {
                return NotFound();
            }
            return View("Edit", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Edit GET for ID {id}");
            return View("Errors");
        }
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Vehicle")] VehicleDto vehicle)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new VehicleBaseViewModel { Vehicle = vehicle };
                // You might need a way to reload dropdowns here if validation fails
                return View("Edit", viewModel);
            }

            var result = await _vehicleService.UpdateVehicleAsync(id, vehicle);

            if (result)
            {
                TempData["SuccessMessage"] = "Vehicle updated successfully";
                return RedirectToAction(nameof(Index));
            }
            
            ModelState.AddModelError("", "Error updating vehicle.");
            var errorViewModel = new VehicleBaseViewModel { Vehicle = vehicle };
            // You might need a way to reload dropdowns here if update fails
            return View("Edit", errorViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"EXCEPTION in Edit POST for ID {id}");
            ModelState.AddModelError("", "An unexpected error occurred");
            var viewModel = new VehicleBaseViewModel { Vehicle = vehicle };
            // You might need a way to reload dropdowns here
            return View("Edit", viewModel);
        }
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var viewModel = await _vehicleService.GetVehicleForCreateAsync();
            return View("Create", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Create GET");
            return View("Errors");
        }
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleBaseViewModel vehicleViewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // The service doesn't have access to ModelState, so we might need to reload dropdowns here
                var freshViewModel = await _vehicleService.GetVehicleForCreateAsync();
                vehicleViewModel.Manufacturers = freshViewModel.Manufacturers;
                vehicleViewModel.FuelTypes = freshViewModel.FuelTypes;
                vehicleViewModel.TransmissionsTypes = freshViewModel.TransmissionsTypes;
                return View("Create", vehicleViewModel);
            }

            var result = await _vehicleService.CreateVehicleAsync(vehicleViewModel);

            if (result)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Error creating vehicle");
            // Reload dropdowns on failure
            var freshViewModelOnFail = await _vehicleService.GetVehicleForCreateAsync();
            vehicleViewModel.Manufacturers = freshViewModelOnFail.Manufacturers;
            vehicleViewModel.FuelTypes = freshViewModelOnFail.FuelTypes;
            vehicleViewModel.TransmissionsTypes = freshViewModelOnFail.TransmissionsTypes;
            return View("Create", vehicleViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Create POST");
            // Reload dropdowns on exception
            var freshViewModelOnFail = await _vehicleService.GetVehicleForCreateAsync();
            vehicleViewModel.Manufacturers = freshViewModelOnFail.Manufacturers;
            vehicleViewModel.FuelTypes = freshViewModelOnFail.FuelTypes;
            vehicleViewModel.TransmissionsTypes = freshViewModelOnFail.TransmissionsTypes;
            return View("Create", vehicleViewModel);
        }
    }
}
