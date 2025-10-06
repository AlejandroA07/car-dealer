using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Manufacturers.Queries.GetById
{
    public class GetManufacturerByIdQuery : IRequest<NamedObjectDto>
    {
        public int Id { get; set; }
    }
}
