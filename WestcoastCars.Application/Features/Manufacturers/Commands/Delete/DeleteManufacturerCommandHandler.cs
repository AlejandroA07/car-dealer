
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Delete;

public class DeleteManufacturerCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteManufacturerCommand>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(DeleteManufacturerCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.ManufacturerRepository ?? throw new InvalidOperationException("Repository for Manufacturer is not available.");
        var manufacturerToDelete = await repository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"Manufacturer with id '{request.Id}' not found.");
        var hasVehicles = await _unitOfWork.VehicleRepository.FirstOrDefaultAsync(v => v.ManufacturerId == request.Id);
        if (hasVehicles is not null)
            throw new ConflictException($"Cannot delete manufacturer '{manufacturerToDelete.Name}' because it has vehicles assigned to it.");

        repository.Remove(manufacturerToDelete!);

        await _unitOfWork.CompleteOrThrowAsync("Failed to delete manufacturer");
    }
}
