using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.Vehicles;
using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.Controllers;

[Route("Vehicles")]
public class VehiclesController : Controller
{
    private readonly IVehicleService _vehicleService;
    private readonly IManufacturerService _manufacturerService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IVehicleService vehicleService, IManufacturerService manufacturerService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _manufacturerService = manufacturerService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<IActionResult> Index([FromQuery] VehicleSearchDto search)
    {
        try
        {
            IList<VehicleSummaryDto> vehicles;

            // Check if any filter is applied (ignoring nulls)
            bool isFiltered = !string.IsNullOrEmpty(search.Make) ||
                              !string.IsNullOrEmpty(search.Model) ||
                              search.MinYear.HasValue ||
                              search.MaxYear.HasValue ||
                              search.MinPrice.HasValue ||
                              search.MaxPrice.HasValue ||
                              search.IsSold.HasValue;

            if (isFiltered)
            {
                // Default to available cars if IsSold is not specified
                if (!search.IsSold.HasValue) search.IsSold = false;
                vehicles = await _vehicleService.SearchVehiclesAsync(search);
            }
            else
            {
                vehicles = await _vehicleService.ListVehiclesAsync();
            }

            var manufacturers = await _manufacturerService.ListAllAsync();
            var manufacturerList = manufacturers.Select(m => new SelectListItem 
            { 
                Value = m.Name, 
                Text = m.Name,
                Selected = m.Name == search.Make
            }).ToList();

            var viewModel = new VehicleListViewModel
            {
                Vehicles = vehicles,
                Search = search,
                Manufacturers = manufacturerList
            };

            return View("Index", viewModel);
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
                TempData["success"] = "Vehicle deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Could not delete vehicle";
            return View("Errors");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Delete for ID {id}");
            TempData["error"] = "An unexpected error occurred";
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
                // Reload dropdowns
                var dropdownData = await _vehicleService.GetVehicleForCreateAsync();
                viewModel.Manufacturers = dropdownData.Manufacturers;
                viewModel.FuelTypes = dropdownData.FuelTypes;
                viewModel.TransmissionsTypes = dropdownData.TransmissionsTypes;
                return View("Edit", viewModel);
            }

            var result = await _vehicleService.UpdateVehicleAsync(id, vehicle);

            if (result)
            {
                TempData["success"] = "Vehicle updated successfully";
                return RedirectToAction(nameof(Index));
            }
            
            TempData["error"] = "Error updating vehicle.";
            var errorViewModel = new VehicleBaseViewModel { Vehicle = vehicle };
            // Reload dropdowns
            var dropdownDataFail = await _vehicleService.GetVehicleForCreateAsync();
            errorViewModel.Manufacturers = dropdownDataFail.Manufacturers;
            errorViewModel.FuelTypes = dropdownDataFail.FuelTypes;
            errorViewModel.TransmissionsTypes = dropdownDataFail.TransmissionsTypes;
            return View("Edit", errorViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"EXCEPTION in Edit POST for ID {id}");
            TempData["error"] = "An unexpected error occurred";
            var viewModel = new VehicleBaseViewModel { Vehicle = vehicle };
            // Reload dropdowns
            var dropdownDataEx = await _vehicleService.GetVehicleForCreateAsync();
            viewModel.Manufacturers = dropdownDataEx.Manufacturers;
            viewModel.FuelTypes = dropdownDataEx.FuelTypes;
            viewModel.TransmissionsTypes = dropdownDataEx.TransmissionsTypes;
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
                TempData["success"] = "Vehicle created successfully";
                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "Error creating vehicle";
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
            TempData["error"] = "An unexpected error occurred";
            // Reload dropdowns on exception
            var freshViewModelOnFail = await _vehicleService.GetVehicleForCreateAsync();
            vehicleViewModel.Manufacturers = freshViewModelOnFail.Manufacturers;
            vehicleViewModel.FuelTypes = freshViewModelOnFail.FuelTypes;
            vehicleViewModel.TransmissionsTypes = freshViewModelOnFail.TransmissionsTypes;
            return View("Create", vehicleViewModel);
        }
    }
}
