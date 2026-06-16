using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Delete;

public class DeleteFuelTypeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteFuelTypeCommand>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(DeleteFuelTypeCommand request, CancellationToken cancellationToken)
    {
        var fuelTypeToDelete = await _unitOfWork.FuelTypeRepository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"FuelType with id '{request.Id}' not found.");
        var hasVehicles = await _unitOfWork.VehicleRepository.FirstOrDefaultAsync(v => v.FuelTypeId == request.Id);
        if (hasVehicles is not null)
            throw new ConflictException($"Cannot delete fuel type '{fuelTypeToDelete.Name}' because it has vehicles assigned to it.");

        _unitOfWork.FuelTypeRepository.Remove(fuelTypeToDelete);

        await _unitOfWork.CompleteOrThrowAsync("Failed to delete fuel type");
    }
}
