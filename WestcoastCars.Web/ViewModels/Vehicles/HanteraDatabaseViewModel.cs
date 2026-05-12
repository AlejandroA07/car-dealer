using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.ViewModels.Vehicles;

public class HanteraDatabaseViewModel
{
    public VehicleStatsSummaryDto Summary { get; set; } = new(0, 0, 0, 0);
    public IEnumerable<VehicleStatsByModelDto> ByModel { get; set; } = [];
    public IEnumerable<VehicleStatsByMileageDto> ByMileage { get; set; } = [];
}
