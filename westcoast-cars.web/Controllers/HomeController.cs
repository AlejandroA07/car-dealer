using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.Vehicles;

namespace westcoast_cars.web.Controllers;

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
        var manufacturers = await _manufacturerService.ListAllAsync();
        var manufacturerList = manufacturers.Select(m => new SelectListItem { Value = m.Name, Text = m.Name }).ToList();
        
        var vehicles = await _vehicleService.ListVehiclesAsync();
        // Take only top 4 for the home page
        var topVehicles = vehicles.Take(4).ToList();

        var viewModel = new VehicleListViewModel
        {
            Manufacturers = manufacturerList,
            Vehicles = topVehicles,
            Search = new WestcoastCars.Contracts.DTOs.VehicleSearchDto()
        };

        return View("Start", viewModel);
    }
}
