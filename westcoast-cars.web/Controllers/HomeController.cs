using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.Vehicles;

namespace westcoast_cars.web.Controllers;

public class HomeController : Controller
{
    private readonly IManufacturerService _manufacturerService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IManufacturerService manufacturerService, ILogger<HomeController> logger)
    {
        _manufacturerService = manufacturerService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var manufacturers = await _manufacturerService.ListAllAsync();
        var manufacturerList = manufacturers.Select(m => new SelectListItem { Value = m.Name, Text = m.Name }).ToList();

        var viewModel = new VehicleListViewModel
        {
            Manufacturers = manufacturerList
        };

        return View("Start", viewModel);
    }
}
