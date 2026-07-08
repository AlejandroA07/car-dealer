using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Web.ViewModels.ServiceBooking;

public class ServiceBookingViewModel
{
    [Required(ErrorMessage = "Registreringsnummer måste anges")]
    [MaxLength(10)]
    [Display(Name = "Registreringsnummer")]
    public string VehicleRegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Välj typ av service")]
    [MaxLength(50)]
    [Display(Name = "Typ av service")]
    public string ServiceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Välj ett tidsfönster")]
    [Display(Name = "Datum för bokning")]
    public DateTime? BookingDate { get; set; }

    [Range(0, 2, ErrorMessage = "Välj ett tidsfönster")]
    public int TimeSlot { get; set; } = -1;

    [Required(ErrorMessage = "Ditt namn måste anges")]
    [MaxLength(100)]
    [Display(Name = "Namn")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-post måste anges")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
    [MaxLength(256)]
    [Display(Name = "E-post")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefonnummer måste anges")]
    [MaxLength(50)]
    [Display(Name = "Telefon")]
    public string CustomerPhone { get; set; } = string.Empty;

    [MaxLength(2000)]
    [Display(Name = "Meddelande (valfritt)")]
    public string Description { get; set; } = string.Empty;

    public string? IdempotencyKey { get; set; }

    public string? VerifiedEmailToken { get; set; }
}
