using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace westcoast_cars.web.ViewModels.Vehicles
{
    public class VehicleBaseViewModel
    {
        [Required(ErrorMessage = "Registeringsnummer saknas!")]
        [DisplayName("Registeringsnummer")]
        public string RegistrationNumber { get; set; }
        public int ManufacturerId { get; set; }
        public List<SelectListItem> Manufacturers { get; set; }
        public int FuelTypeId { get; set; }
        public List<SelectListItem> FuelTypes { get; set; }
        public int TransmissionTypeId { get; set; }
        public List<SelectListItem> TransmissionsTypes { get; set; }
        [Required(ErrorMessage = "Modell typ saknas!")]
        [DisplayName("Modell typ")]
        public string Model { get; set; }
        [Required(ErrorMessage = "Årsmodell saknas")]
        [DisplayName("Årsmodell")]
        public string ModelYear { get; set; }
        [Required(ErrorMessage = "Antal km saknas!")]
        [Range(1, 500000, ErrorMessage = "Antal körda km saknas och får ej vara större än 50000 mil")]
        [DisplayName("Antal km")]
        public int Mileage { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}