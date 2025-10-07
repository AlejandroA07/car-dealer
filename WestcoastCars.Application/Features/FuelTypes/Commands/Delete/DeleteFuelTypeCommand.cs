using MediatR;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Delete
{
    public class DeleteFuelTypeCommand : IRequest
    {
        public int Id { get; set; }
    }
}
