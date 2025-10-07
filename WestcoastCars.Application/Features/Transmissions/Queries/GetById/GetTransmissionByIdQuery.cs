using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Transmissions.Queries.GetById
{
    public class GetTransmissionByIdQuery : IRequest<NamedObjectDto?>
    {
        public int Id { get; set; }
    }
}
