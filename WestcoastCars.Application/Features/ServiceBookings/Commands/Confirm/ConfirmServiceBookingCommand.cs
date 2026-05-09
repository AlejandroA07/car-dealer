using MediatR;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Confirm;

public class ConfirmServiceBookingCommand : IRequest<Unit>
{
    public int Id { get; set; }
}
