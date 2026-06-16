using MediatR;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.PurgeSourceRemoved;

public class PurgeSourceRemovedVehiclesCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<PurgeSourceRemovedVehiclesCommand, PurgeSourceRemovedVehiclesResult>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PurgeSourceRemovedVehiclesResult> Handle(PurgeSourceRemovedVehiclesCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _unitOfWork.VehicleRepository.PurgeSourceRemovedAsync(cancellationToken);
        return new PurgeSourceRemovedVehiclesResult(deleted);
    }
}
