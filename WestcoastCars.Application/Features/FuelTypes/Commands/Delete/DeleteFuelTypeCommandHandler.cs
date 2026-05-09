using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Delete;

public class DeleteFuelTypeCommandHandler : IRequestHandler<DeleteFuelTypeCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFuelTypeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteFuelTypeCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.FuelTypeRepository;
        if (repository is null) throw new InvalidOperationException("Repository for FuelType is not available.");

        var fuelTypeToDelete = await repository.GetByIdAsync(request.Id);

        if (fuelTypeToDelete is null)
        {
            throw new NotFoundException($"FuelType with id '{request.Id}' not found.");
        }

        var hasVehicles = await _unitOfWork.VehicleRepository.FirstOrDefaultAsync(v => v.FuelTypeId == request.Id);
        if (hasVehicles is not null)
            throw new ConflictException($"Cannot delete fuel type '{fuelTypeToDelete.Name}' because it has vehicles assigned to it.");

        repository.Remove(fuelTypeToDelete!);

        await _unitOfWork.CompleteOrThrowAsync("Failed to delete fuel type");
    }
}
