using MediatR;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.BulkDelete;

public class BulkDeleteVehiclesCommandHandler : IRequestHandler<BulkDeleteVehiclesCommand, BulkDeleteVehiclesResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public BulkDeleteVehiclesCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkDeleteVehiclesResult> Handle(BulkDeleteVehiclesCommand request, CancellationToken cancellationToken)
    {
        if (request.Make is null && request.Model is null && request.IsSold is null && request.MinMileage is null && request.MaxMileage is null)
            throw new InvalidOperationException("At least one filter must be specified for bulk delete.");

        var vehicles = await _unitOfWork.VehicleRepository.GetForBulkDeleteAsync(
            request.Make, request.Model, request.IsSold, request.MinMileage, request.MaxMileage);

        if (vehicles.Count > 0)
        {
            _unitOfWork.VehicleRepository.RemoveRange(vehicles);
            await _unitOfWork.CompleteAsync();
        }

        return new BulkDeleteVehiclesResult(vehicles.Count);
    }
}
