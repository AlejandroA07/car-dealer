using System.ComponentModel.DataAnnotations;

namespace westcoast_cars.web.ViewModels.ServiceBooking
{
    public class ServiceBookingViewModel
    {
        [Required(ErrorMessage = "Registreringsnummer måste anges")]
        [Display(Name = "Registreringsnummer")]
        public string VehicleRegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Välj typ av service")]
        [Display(Name = "Typ av service")]
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Välj ett datum")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum för bokning")]
        public DateTime BookingDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "Ditt namn måste anges")]
        [Display(Name = "Namn")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-post måste anges")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        [Display(Name = "E-post")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefonnummer måste anges")]
        [Display(Name = "Telefon")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Display(Name = "Meddelande (valfritt)")]
        public string Description { get; set; } = string.Empty;
    }
}
