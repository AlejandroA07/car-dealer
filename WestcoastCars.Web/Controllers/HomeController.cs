using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels.Vehicles;

namespace WestcoastCars.Web.Controllers;

public class HomeController : Controller
{
    private readonly IVehicleService _vehicleService;
    private readonly IManufacturerService _manufacturerService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IVehicleService vehicleService, IManufacturerService manufacturerService, ILogger<HomeController> logger)
    {
        _vehicleService = vehicleService;
        _manufacturerService = manufacturerService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var manufacturerList = new List<SelectListItem>();
        var topVehicles = new List<WestcoastCars.Contracts.DTOs.VehicleSummaryDto>();

        try
        {
            var manufacturersTask = _manufacturerService.ListAllAsync();
            var vehiclesTask = _vehicleService.ListVehiclesAsync(pageSize: 4);

            await Task.WhenAll(manufacturersTask, vehiclesTask);

            manufacturerList = manufacturersTask.Result
                .Select(m => new SelectListItem { Value = m.Name, Text = m.Name })
                .ToList();

            topVehicles = vehiclesTask.Result.Items;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "API is unavailable while loading home page data");
        }

        var viewModel = new VehicleListViewModel
        {
            Manufacturers = manufacturerList,
            Vehicles = topVehicles,
            Search = new WestcoastCars.Contracts.DTOs.VehicleSearchDto()
        };

        return View("Start", viewModel);
    }
}
