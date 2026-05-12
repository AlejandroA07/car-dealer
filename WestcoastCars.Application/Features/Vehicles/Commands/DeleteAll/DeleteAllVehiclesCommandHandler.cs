using MediatR;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.DeleteAll;

public class DeleteAllVehiclesCommandHandler : IRequestHandler<DeleteAllVehiclesCommand, DeleteAllVehiclesResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAllVehiclesCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteAllVehiclesResult> Handle(DeleteAllVehiclesCommand request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.VehicleRepository.GetAllForDeleteAsync();

        if (vehicles.Count > 0)
        {
            _unitOfWork.VehicleRepository.RemoveRange(vehicles);
            await _unitOfWork.CompleteAsync();
        }

        return new DeleteAllVehiclesResult(vehicles.Count);
    }
}
