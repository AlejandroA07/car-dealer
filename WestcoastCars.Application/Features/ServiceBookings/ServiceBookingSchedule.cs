using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Application.Features.ServiceBookings;

internal static class ServiceBookingSchedule
{
    public static readonly IReadOnlyDictionary<TimeSlot, TimeOnly> SlotEndTimes = new Dictionary<TimeSlot, TimeOnly>
    {
        [TimeSlot.Morning] = new(10, 0),
        [TimeSlot.MidMorning] = new(12, 0),
        [TimeSlot.Afternoon] = new(15, 0)
    };

    public static bool HasSlotPassed(DateTime localNow, DateTime bookingDate, TimeSlot slot)
    {
        var bookingDay = DateOnly.FromDateTime(bookingDate);
        var currentDay = DateOnly.FromDateTime(localNow);

        if (bookingDay < currentDay)
            return true;

        if (bookingDay > currentDay)
            return false;

        return localNow.TimeOfDay >= SlotEndTimes[slot].ToTimeSpan();
    }

    public static DateOnly GetMonday(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
