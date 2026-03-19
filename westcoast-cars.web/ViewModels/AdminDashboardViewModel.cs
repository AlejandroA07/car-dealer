using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalVehicles { get; set; }
    public int SoldVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<VehicleSummaryDto> RecentVehicles { get; set; } = new();
    public List<ManufacturerStockSummary> StockByManufacturer { get; set; } = new();
}

public class ManufacturerStockSummary
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
