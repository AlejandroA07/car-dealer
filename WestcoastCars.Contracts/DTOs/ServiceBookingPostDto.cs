using System;
using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.DTOs
{
    public class ServiceBookingPostDto
    {
        [Required(ErrorMessage = "Registreringsnummer måste anges")]
        public string VehicleRegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Typ av service måste anges")]
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum för bokning måste anges")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Namn måste anges")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-post måste anges")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefonnummer måste anges")]
        public string CustomerPhone { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
