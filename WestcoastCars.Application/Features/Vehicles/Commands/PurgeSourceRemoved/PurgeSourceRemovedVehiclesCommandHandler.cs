using MediatR;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.PurgeSourceRemoved;

public class PurgeSourceRemovedVehiclesCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<PurgeSourceRemovedVehiclesCommand, PurgeSourceRemovedVehiclesResult>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PurgeSourceRemovedVehiclesResult> Handle(PurgeSourceRemovedVehiclesCommand request, CancellationToken cancellationToken)
    {
        var vehicles = (await _unitOfWork.VehicleRepository.GetAllSourceRemovedFromBlocketAsync()).ToList();

        if (vehicles.Count > 0)
        {
            _unitOfWork.VehicleRepository.RemoveRange(vehicles);
            await _unitOfWork.CompleteAsync();
        }

        return new PurgeSourceRemovedVehiclesResult(vehicles.Count);
    }
}
