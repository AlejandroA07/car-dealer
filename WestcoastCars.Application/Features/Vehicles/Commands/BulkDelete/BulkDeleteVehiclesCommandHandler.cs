using MediatR;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.BulkDelete;

public class BulkDeleteVehiclesCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<BulkDeleteVehiclesCommand, BulkDeleteVehiclesResult>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BulkDeleteVehiclesResult> Handle(BulkDeleteVehiclesCommand request, CancellationToken cancellationToken)
    {
        if (request.Make is null && request.Model is null && request.IsSold is null && request.MinMileage is null && request.MaxMileage is null)
            throw new InvalidOperationException("At least one filter must be specified for bulk delete.");

        var deleted = await _unitOfWork.VehicleRepository.BulkDeleteAsync(
            request.Make, request.Model, request.IsSold, request.MinMileage, request.MaxMileage, cancellationToken);

        return new BulkDeleteVehiclesResult(deleted);
    }
}
