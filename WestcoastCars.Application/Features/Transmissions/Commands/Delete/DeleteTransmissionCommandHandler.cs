using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Delete;

public class DeleteTransmissionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteTransmissionCommand>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(DeleteTransmissionCommand request, CancellationToken cancellationToken)
    {
        var transmissionTypeToDelete = await _unitOfWork.TransmissionTypeRepository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"TransmissionType with id '{request.Id}' not found.");
        var hasVehicles = await _unitOfWork.VehicleRepository.FirstOrDefaultAsync(v => v.TransmissionTypeId == request.Id);
        if (hasVehicles is not null)
            throw new ConflictException($"Cannot delete transmission type '{transmissionTypeToDelete.Name}' because it has vehicles assigned to it.");

        _unitOfWork.TransmissionTypeRepository.Remove(transmissionTypeToDelete);

        await _unitOfWork.CompleteOrThrowAsync("Failed to delete transmission type");
    }
}
