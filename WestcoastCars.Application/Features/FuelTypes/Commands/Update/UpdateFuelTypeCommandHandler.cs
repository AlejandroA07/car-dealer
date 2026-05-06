using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Update;

public class UpdateFuelTypeCommandHandler : IRequestHandler<UpdateFuelTypeCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFuelTypeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateFuelTypeCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.FuelTypeRepository;
        if (repository is null) throw new InvalidOperationException("Repository for FuelType is not available.");

        var fuelTypeToUpdate = await repository.GetByIdAsync(request.Id);

        if (fuelTypeToUpdate is null)
        {
            throw new NotFoundException($"FuelType with id '{request.Id}' not found.");
        }

        await repository.ThrowIfNameExistsAsync(request.Name, nameof(FuelType), request.Id);

        fuelTypeToUpdate!.Name = request.Name;
        repository.Update(fuelTypeToUpdate!);

        await _unitOfWork.CompleteOrThrowAsync("Failed to update fuel type");
    }
}
