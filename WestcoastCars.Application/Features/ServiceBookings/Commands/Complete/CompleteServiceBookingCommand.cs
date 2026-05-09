using MediatR;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Complete;

public class CompleteServiceBookingCommand : IRequest<Unit>
{
    public int Id { get; set; }
}
