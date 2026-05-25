using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.DTOs;

public class CancelServiceBookingDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string CancellationReason { get; set; } = string.Empty;
}
