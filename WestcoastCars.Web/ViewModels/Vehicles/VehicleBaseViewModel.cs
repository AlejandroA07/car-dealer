
using Microsoft.AspNetCore.Mvc.Rendering;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.ViewModels.Vehicles;

public class VehicleBaseViewModel
{
    public VehicleDto Vehicle { get; set; } = new VehicleDto();
    public List<SelectListItem> Manufacturers { get; set; } = [];
    public List<SelectListItem> FuelTypes { get; set; } = [];
    public List<SelectListItem> TransmissionTypes { get; set; } = [];
}
