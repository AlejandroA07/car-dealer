
using MediatR;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Delete
{
    public class DeleteManufacturerCommand : IRequest
    {
        public int Id { get; set; }
    }
}
