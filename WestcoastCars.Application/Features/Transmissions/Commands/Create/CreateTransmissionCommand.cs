using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Create
{
    public class CreateTransmissionCommand : IRequest<NamedObjectDto?>
    {
        public required string Name { get; set; }
    }
}
