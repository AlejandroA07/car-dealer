using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.DTOs
{
    /// <summary>
    /// Represents a detailed view of a vehicle.
    /// </summary>
    public class VehicleDto
    {
        /// <summary>
        /// Unique identifier for the vehicle.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Vehicle registration number (plate).
        /// </summary>
        [Required(ErrorMessage = "RegistrationNumber måste anges")]
        public string RegistrationNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID of the manufacturer (Make).
        /// </summary>
        [Required(ErrorMessage = "Tillverkare måste anges")]
        public int ManufacturerId { get; set; }

        /// <summary>
        /// Model name (e.g., "Model S", "Golf").
        /// </summary>
        [Required(ErrorMessage = "Bilmodell måste anges")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Production year or model year.
        /// </summary>
        [Required(ErrorMessage = "Årsmodell måste anges")]
        public string ModelYear { get; set; } = string.Empty;

        /// <summary>
        /// Current mileage in kilometers.
        /// </summary>
        [Required(ErrorMessage = "Antal körda km måste anges")]
        public int Mileage { get; set; }

        /// <summary>
        /// ID of the fuel type.
        /// </summary>
        [Required(ErrorMessage = "Bränsletyp måste anges")]
        public int FuelTypeId { get; set; }

        /// <summary>
        /// ID of the transmission type.
        /// </summary>
        [Required(ErrorMessage = "Typ av växellåda måste anges")]
        public int TransmissionTypeId { get; set; }

        /// <summary>
        /// Selling price or value of the vehicle.
        /// </summary>
        [Required(ErrorMessage = "Värde på bilen måste anges")]
        public int Value { get; set; }

        /// <summary>
        /// Detailed description of the vehicle's condition and features.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the vehicle has been sold.
        /// </summary>
        public bool IsSold { get; set; } = false;

        /// <summary>
        /// Relative path or URL to the vehicle's primary image.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}
