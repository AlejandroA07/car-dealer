using MediatR;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Cancel;

public class CancelServiceBookingCommand : IRequest<Unit>
{
    public int Id { get; set; }
}
