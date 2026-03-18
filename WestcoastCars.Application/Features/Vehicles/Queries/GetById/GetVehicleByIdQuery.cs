using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.GetById
{
    public class GetVehicleByIdQuery : IRequest<VehicleDetailsDto>
    {
        public int Id { get; set; }
    }
}
