using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "RegistrationNumber måste anges")]
        public string RegistrationNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tillverkare måste anges")]
        public int ManufacturerId { get; set; }
        [Required(ErrorMessage = "Bilmodell måste anges")]
        public string Model { get; set; } = string.Empty;
        [Required(ErrorMessage = "Årsmodell måste anges")]
        public string ModelYear { get; set; } = string.Empty;
        [Required(ErrorMessage = "Antal körda km måste anges")]
        public int Mileage { get; set; }
        [Required(ErrorMessage = "Bränsletyp måste anges")]
        public int FuelTypeId { get; set; }
        [Required(ErrorMessage = "Typ av växellåda måste anges")]
        public int TransmissionTypeId { get; set; }
        [Required(ErrorMessage = "Värde på bilen måste anges")]
        public int Value { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsSold { get; set; } = false;
        public string ImageUrl { get; set; } = string.Empty;
    }
}