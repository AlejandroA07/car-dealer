using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels.Vehicles;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.Controllers;

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

    [HttpGet("list", Name = "VehicleCatalog")]
    public async Task<IActionResult> Index([FromQuery] VehicleSearchDto search, [FromQuery] int page = 1)
    {
        try
        {
            search.Page = page;
            const int pageSize = 15;
            search.PageSize = pageSize;
            PagedResult<VehicleSummaryDto> result;

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
                result = await _vehicleService.SearchVehiclesAsync(search);
            }
            else
            {
                result = await _vehicleService.ListVehiclesAsync(page, pageSize);
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
                Vehicles = result.Items,
                Search = search,
                Manufacturers = manufacturerList,
                CurrentPage = result.Page,
                PageSize = result.PageSize,
                TotalVehicles = result.TotalCount
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Index");
            return View("Errors");
        }
    }

    [Authorize(Roles = "Admin,Salesperson")]
    [HttpGet("sync-blocket")]
    public IActionResult SyncBlocket()
    {
        return View("SyncBlocket", new BlocketSyncViewModel());
    }

    [Authorize(Roles = "Admin,Salesperson")]
    [HttpPost("sync-blocket")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncBlocket(BlocketSyncViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View("SyncBlocket", model);
            }

            var result = await _vehicleService.SyncBlocketAsync(model);

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                TempData["error"] = result.ErrorMessage;
            }
            else if (!string.IsNullOrWhiteSpace(result.InfoMessage))
            {
                TempData["info"] = result.InfoMessage;
            }
            else
            {
                TempData["success"] = $"Blocket sync completed. Imported {result.TotalImported} vehicles.";
            }

            return View("SyncBlocket", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Blocket sync");
            model.ErrorMessage = "An unexpected error occurred while running the Blocket sync.";
            TempData["error"] = model.ErrorMessage;
            return View("SyncBlocket", model);
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
                viewModel.TransmissionTypes = dropdownData.TransmissionTypes;
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
            errorViewModel.TransmissionTypes = dropdownDataFail.TransmissionTypes;
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
            viewModel.TransmissionTypes = dropdownDataEx.TransmissionTypes;
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
                vehicleViewModel.TransmissionTypes = freshViewModel.TransmissionTypes;
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
            vehicleViewModel.TransmissionTypes = freshViewModelOnFail.TransmissionTypes;
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
            vehicleViewModel.TransmissionTypes = freshViewModelOnFail.TransmissionTypes;
            return View("Create", vehicleViewModel);
        }
    }
}
