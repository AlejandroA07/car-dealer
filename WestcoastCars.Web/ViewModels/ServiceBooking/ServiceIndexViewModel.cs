using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.ViewModels.ServiceBooking;

public class ServiceIndexViewModel
{
    public ServiceBookingViewModel BookingForm { get; set; } = new();
    public DateOnly WeekStart { get; set; }
    public List<SlotAvailabilityDto> WeekSlots { get; set; } = [];
    public bool AvailabilityLoadFailed { get; set; }
    public string AvailabilityErrorMessage { get; set; } = string.Empty;
}
