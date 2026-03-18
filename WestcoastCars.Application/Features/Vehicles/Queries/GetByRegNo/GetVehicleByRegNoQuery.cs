using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.GetByRegNo
{
    public class GetVehicleByRegNoQuery : IRequest<VehicleDetailsDto>
    {
        public string RegistrationNumber { get; set; } = string.Empty;
    }
}
