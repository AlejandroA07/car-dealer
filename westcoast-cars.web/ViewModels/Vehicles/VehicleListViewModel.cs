using Microsoft.AspNetCore.Mvc.Rendering;
using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.ViewModels.Vehicles;

public class VehicleListViewModel
{
    public IList<VehicleSummaryDto> Vehicles { get; set; } = new List<VehicleSummaryDto>();
    public VehicleSearchDto Search { get; set; } = new VehicleSearchDto();
    public IEnumerable<SelectListItem> Manufacturers { get; set; } = new List<SelectListItem>();
}
