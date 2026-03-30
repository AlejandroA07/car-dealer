
using Microsoft.AspNetCore.Mvc.Rendering;
using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.ViewModels.Vehicles
{
    public class VehicleBaseViewModel
    {
        public VehicleDto Vehicle { get; set; } = new VehicleDto();
        public List<SelectListItem> Manufacturers { get; set; } = new();
        public List<SelectListItem> FuelTypes { get; set; } = new();
        public List<SelectListItem> TransmissionsTypes { get; set; } = new();
    }
}
