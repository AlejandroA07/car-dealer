using MediatR;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.DeleteAll;

public class DeleteAllVehiclesCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteAllVehiclesCommand, DeleteAllVehiclesResult>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<DeleteAllVehiclesResult> Handle(DeleteAllVehiclesCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _unitOfWork.VehicleRepository.DeleteAllAsync(cancellationToken);
        return new DeleteAllVehiclesResult(deleted);
    }
}
