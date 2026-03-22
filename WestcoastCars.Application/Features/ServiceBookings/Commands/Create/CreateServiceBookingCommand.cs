using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Create
{
    public class CreateServiceBookingCommand : ServiceBookingPostDto, IRequest<int>
    {
    }
}
