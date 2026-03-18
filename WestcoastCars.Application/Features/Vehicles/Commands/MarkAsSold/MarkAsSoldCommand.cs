using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.MarkAsSold
{
    public class MarkAsSoldCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }
}
