using Microsoft.AspNetCore.Mvc.Rendering;
using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.ViewModels.Vehicles;

public class VehicleListViewModel
{
    public IList<VehicleSummaryDto> Vehicles { get; set; } = new List<VehicleSummaryDto>();
    public VehicleSearchDto Search { get; set; } = new VehicleSearchDto();
    public IEnumerable<SelectListItem> Manufacturers { get; set; } = new List<SelectListItem>();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalVehicles { get; set; }
    public int TotalPages => TotalVehicles == 0 ? 1 : (int)Math.Ceiling(TotalVehicles / (double)PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
