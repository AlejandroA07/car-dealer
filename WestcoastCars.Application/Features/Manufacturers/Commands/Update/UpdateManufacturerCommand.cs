
using MediatR;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Update
{
    public class UpdateManufacturerCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
