using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Update;

public class UpdateFuelTypeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateFuelTypeCommand>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(UpdateFuelTypeCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.FuelTypeRepository ?? throw new InvalidOperationException("Repository for FuelType is not available.");
        var fuelTypeToUpdate = await repository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"FuelType with id '{request.Id}' not found.");
        fuelTypeToUpdate!.Name = request.Name;
        repository.Update(fuelTypeToUpdate!);

        await _unitOfWork.CompleteOrThrowAsync("Failed to update fuel type");
    }
}
