using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels;

namespace WestcoastCars.Web.Controllers;

[Route("[controller]")]
[Authorize(Roles = "Admin,Salesperson")]
public class AdminController(IVehicleService vehicleService) : Controller
{
    private readonly IVehicleService _vehicleService = vehicleService;

    public async Task<IActionResult> Index()
    {
        var vehicles = await _vehicleService.ListAllVehiclesAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalVehicles = vehicles.Count,
            SoldVehicles = vehicles.Count(v => v.IsSold),
            AvailableVehicles = vehicles.Count(v => !v.IsSold),
            TotalInventoryValue = vehicles.Sum(v => v.Price),
            RecentVehicles = [.. vehicles.OrderByDescending(v => v.Id).Take(5)],
            StockByManufacturer = [.. vehicles
                .GroupBy(v => v.Manufacturer)
                .Select(g => new ManufacturerStockSummary
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)]
        };

        return View("Admin", viewModel);
    }
}
