using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Delete
{
    public class DeleteVehicleCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }
}
