using Microsoft.AspNetCore.Mvc.Rendering;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.ViewModels.Vehicles;

public class VehicleListViewModel
{
    public IList<VehicleSummaryDto> Vehicles { get; set; } = [];
    public VehicleSearchDto Search { get; set; } = new VehicleSearchDto();
    public IEnumerable<SelectListItem> Manufacturers { get; set; } = [];
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalVehicles { get; set; }
    public int TotalPages => TotalVehicles == 0 ? 1 : (int)Math.Ceiling(TotalVehicles / (double)PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
