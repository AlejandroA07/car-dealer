using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Create
{
    public class CreateFuelTypeCommand : IRequest<NamedObjectDto?>
    {
        public required string Name { get; set; }
    }
}
