using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.ServiceBookings.Queries.GetWeekSlots;

public class GetWeekSlotsQuery : IRequest<IEnumerable<SlotAvailabilityDto>>
{
    public DateOnly WeekStart { get; set; }
}
