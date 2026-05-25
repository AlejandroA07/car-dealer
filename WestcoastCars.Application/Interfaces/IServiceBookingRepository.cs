using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IServiceBookingRepository : IRepository<ServiceBooking>
{
    Task<PagedResult<ServiceBooking>> GetPagedAsync(PagedQueryDto pagination, bool? isActive = null);
    Task<bool> IsSlotTakenAsync(DateOnly date, TimeSlot slot);
    Task<bool> HasActiveBookingForRegistrationAsync(string registrationNumber);
    Task<IReadOnlySet<(DateOnly Date, TimeSlot Slot)>> GetBookedSlotsForRangeAsync(DateOnly from, DateOnly to);
}
