using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Create
{
    public class CreateManufacturerCommand : IRequest<NamedObjectDto>
    {
        public string Name { get; set; }
    }
}
