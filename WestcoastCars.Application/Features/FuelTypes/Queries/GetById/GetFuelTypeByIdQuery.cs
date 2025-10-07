using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.FuelTypes.Queries.GetById
{
    public class GetFuelTypeByIdQuery : IRequest<NamedObjectDto?>
    {
        public int Id { get; set; }
    }
}
