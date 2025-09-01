using System.ComponentModel.DataAnnotations;

namespace westcoast_cars.api.ViewModels
{
    public class VehicleBaseViewModel
    {
        [Required(ErrorMessage = "Tillverkare måste anges")]
        public int ManufacturerId { get; set; } // Changed from string Make
        [Required(ErrorMessage = "Bilmodell måste anges")]
        public string Model { get; set; }
        [Required(ErrorMessage = "Årsmodell måste anges")]
        public string ModelYear { get; set; }
        [Required(ErrorMessage = "Antal körda km måste anges")]
        public int Mileage { get; set; }
        [Required(ErrorMessage = "Bränsletyp måste anges")]
        public int FuelTypeId { get; set; } // Changed from string FuelType
        [Required(ErrorMessage = "Typ av växellåda måste anges")]
        public int TransmissionTypeId { get; set; } // Changed from string Transmission
        [Required(ErrorMessage = "Värde på bilen måste anges")]
        public int Value { get; set; }
        public string Description { get; set; }
        public bool IsSold { get; set; } = false;
        public string ImageUrl { get; set; }
    }
}
