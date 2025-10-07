using MediatR;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Update
{
    public class UpdateFuelTypeCommand : IRequest
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
