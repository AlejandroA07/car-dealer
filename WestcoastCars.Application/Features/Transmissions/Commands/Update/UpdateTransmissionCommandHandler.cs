using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Update;

public class UpdateTransmissionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateTransmissionCommand>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(UpdateTransmissionCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.TransmissionTypeRepository ?? throw new InvalidOperationException("Repository for TransmissionType is not available.");
        var transmissionTypeToUpdate = await repository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"TransmissionType with id '{request.Id}' not found.");
        transmissionTypeToUpdate!.Name = request.Name;
        repository.Update(transmissionTypeToUpdate!);

        await _unitOfWork.CompleteOrThrowAsync("Failed to update transmission type");
    }
}
