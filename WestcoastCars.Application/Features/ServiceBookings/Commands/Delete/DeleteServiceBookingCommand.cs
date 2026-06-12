using MediatR;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Delete;

public class DeleteServiceBookingCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
