using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Delete;

public class DeleteVehicleCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteVehicleCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Unit> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"Vehicle with ID {request.Id} not found");
        _unitOfWork.VehicleRepository.Remove(vehicle);

        await _unitOfWork.CompleteOrThrowAsync("Failed to delete vehicle");
        return Unit.Value;
    }
}
