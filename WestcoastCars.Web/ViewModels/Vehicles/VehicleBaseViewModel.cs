
using Microsoft.AspNetCore.Mvc.Rendering;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.ViewModels.Vehicles;

public class VehicleBaseViewModel
{
    public VehicleDto Vehicle { get; set; } = new VehicleDto();
    public List<SelectListItem> Manufacturers { get; set; } = [];
    public List<SelectListItem> FuelTypes { get; set; } = [];
    public List<SelectListItem> TransmissionTypes { get; set; } = [];

    // View-model only — not part of the shared VehicleDto/Api contract. When set, takes
    // precedence over Vehicle.ImageUrl in the Create/Edit POST actions.
    public IFormFile? ImageFile { get; set; }
}
