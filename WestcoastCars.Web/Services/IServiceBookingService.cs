using WestcoastCars.Web.ViewModels.ServiceBooking;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.Services;

public interface IServiceBookingService
{
    Task<ServiceBookingActionResult> CreateBookingAsync(ServiceBookingViewModel model);
    Task<ServiceBookingDataResult<IReadOnlyList<ServiceBookingSummaryDto>>> ListActiveBookingsAsync();
    Task<ServiceBookingDataResult<IReadOnlyList<ServiceBookingSummaryDto>>> ListInactiveBookingsAsync();
    Task<ServiceBookingDataResult<IReadOnlyList<ServiceBookingSummaryDto>>> ListAllBookingsAsync();
    Task<ServiceBookingDataResult<IReadOnlyList<SlotAvailabilityDto>>> GetWeekSlotsAsync(DateOnly weekStart);
    Task<ServiceBookingActionResult> CancelAsync(int id, string cancellationReason);
    Task<ServiceBookingActionResult> CompleteAsync(int id);
    Task<ServiceBookingActionResult> DeleteAsync(int id);
}
