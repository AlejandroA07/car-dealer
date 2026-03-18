using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Update
{
    public class UpdateVehicleCommand : VehicleUpdateDto, IRequest<Unit>
    {
    }
}
