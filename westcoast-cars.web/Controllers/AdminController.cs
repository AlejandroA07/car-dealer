using Microsoft.AspNetCore.Mvc;
using westcoast_cars.web.Services;
using westcoast_cars.web.ViewModels.Admin;

namespace westcoast_cars.web.Controllers
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public AdminController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleService.ListAllVehiclesAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalVehicles = vehicles.Count,
                SoldVehicles = vehicles.Count(v => v.IsSold),
                AvailableVehicles = vehicles.Count(v => !v.IsSold),
                TotalInventoryValue = vehicles.Sum(v => v.Value),
                RecentVehicles = vehicles.OrderByDescending(v => v.Id).Take(5).ToList(),
                StockByManufacturer = vehicles
                    .GroupBy(v => v.Manufacturer)
                    .Select(g => new ManufacturerStockSummary
                    {
                        Name = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList()
            };

            return View("Admin", viewModel);
        }
    }
}