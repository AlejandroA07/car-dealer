using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Update;

public class UpdateTransmissionCommandHandler : IRequestHandler<UpdateTransmissionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTransmissionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateTransmissionCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.TransmissionTypeRepository;
        if (repository is null) throw new InvalidOperationException("Repository for TransmissionType is not available.");

        var transmissionTypeToUpdate = await repository.GetByIdAsync(request.Id);

        if (transmissionTypeToUpdate is null)
        {
            throw new NotFoundException($"TransmissionType with id '{request.Id}' not found.");
        }

        await repository.ThrowIfNameExistsAsync(request.Name, nameof(TransmissionType), request.Id);

        transmissionTypeToUpdate!.Name = request.Name;
        repository.Update(transmissionTypeToUpdate!);

        await _unitOfWork.CompleteOrThrowAsync("Failed to update transmission type");
    }
}
