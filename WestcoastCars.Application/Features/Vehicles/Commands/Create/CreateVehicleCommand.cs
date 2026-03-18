using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Create
{
    public class CreateVehicleCommand : VehiclePostDto, IRequest<int>
    {
    }
}
