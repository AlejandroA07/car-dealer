using MediatR;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Application.Features.ServiceBookings.Queries.GetWeekSlots;

public class GetWeekSlotsQueryHandler : IRequestHandler<GetWeekSlotsQuery, IEnumerable<SlotAvailabilityDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetWeekSlotsQueryHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IEnumerable<SlotAvailabilityDto>> Handle(GetWeekSlotsQuery request, CancellationToken cancellationToken)
    {
        var normalizedWeekStart = ServiceBookingSchedule.GetMonday(request.WeekStart);
        var currentMonday = ServiceBookingSchedule.GetMonday(DateOnly.FromDateTime(_dateTimeProvider.LocalNow));
        var maxMonday = currentMonday.AddDays(42);

        if (normalizedWeekStart < currentMonday || normalizedWeekStart > maxMonday)
        {
            throw new ValidationException(
                nameof(request.WeekStart),
                ["Veckan måste ligga inom innevarande vecka eller de kommande sex veckorna."]);
        }

        var weekEnd = normalizedWeekStart.AddDays(4);
        var bookedSlots = await _unitOfWork.ServiceBookingRepository.GetBookedSlotsForRangeAsync(normalizedWeekStart, weekEnd);

        var result = new List<SlotAvailabilityDto>(15);
        for (var d = 0; d < 5; d++)
        {
            var date = normalizedWeekStart.AddDays(d);
            foreach (TimeSlot slot in Enum.GetValues<TimeSlot>())
            {
                result.Add(new SlotAvailabilityDto
                {
                    Date = date,
                    TimeSlot = (int)slot,
                    IsBooked = bookedSlots.Contains((date, slot))
                });
            }
        }

        return result;
    }
}
