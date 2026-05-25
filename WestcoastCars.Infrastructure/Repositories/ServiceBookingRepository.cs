using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class ServiceBookingRepository : Repository<ServiceBooking>, IServiceBookingRepository
{
    private const int MaxPageSize = 100;

    public ServiceBookingRepository(WestcoastCarsContext context) : base(context)
    {
    }

    public async Task<PagedResult<ServiceBooking>> GetPagedAsync(PagedQueryDto pagination, bool? isActive = null)
    {
        var page = Math.Max(1, pagination.Page);
        var pageSize = Math.Clamp(pagination.PageSize, 1, MaxPageSize);

        var query = _context.ServiceBookings.AsNoTracking();

        if (isActive.HasValue)
        {
            query = isActive.Value
                ? ApplyActiveFilter(query)
                : ApplyInactiveFilter(query);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ServiceBooking>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> IsSlotTakenAsync(DateOnly date, TimeSlot slot)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var dayEnd = dayStart.AddDays(1);

        return await ApplyActiveFilter(_context.ServiceBookings.AsNoTracking())
            .AnyAsync(b =>
                b.BookingDate >= dayStart &&
                b.BookingDate < dayEnd &&
                b.TimeSlot == slot);
    }

    public async Task<bool> HasActiveBookingForRegistrationAsync(string normalizedRegistrationNumber)
    {
        return await ApplyActiveFilter(_context.ServiceBookings.AsNoTracking())
            .AnyAsync(b => b.VehicleRegistrationNumber == normalizedRegistrationNumber);
    }

    public async Task<IReadOnlySet<(DateOnly Date, TimeSlot Slot)>> GetBookedSlotsForRangeAsync(DateOnly from, DateOnly to)
    {
        var rangeStart = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var rangeEnd = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).AddDays(1);

        var booked = await ApplyActiveFilter(_context.ServiceBookings.AsNoTracking())
            .Where(b =>
                b.BookingDate >= rangeStart &&
                b.BookingDate < rangeEnd)
            .Select(b => new { b.BookingDate, b.TimeSlot })
            .ToListAsync();

        return booked
            .Select(b => (DateOnly.FromDateTime(b.BookingDate), b.TimeSlot))
            .ToHashSet();
    }

    private static IQueryable<ServiceBooking> ApplyActiveFilter(IQueryable<ServiceBooking> query)
    {
        return query.Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Completed);
    }

    private static IQueryable<ServiceBooking> ApplyInactiveFilter(IQueryable<ServiceBooking> query)
    {
        return query.Where(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Completed);
    }
}
