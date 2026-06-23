using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Create;

public class CreateTransmissionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateTransmissionCommand, NamedObjectDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<NamedObjectDto> Handle(CreateTransmissionCommand request, CancellationToken cancellationToken)
    {
        var transmissionTypeToAdd = new TransmissionType { Name = request.Name };
        await _unitOfWork.TransmissionTypeRepository.AddAsync(transmissionTypeToAdd);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create transmission type");
        return transmissionTypeToAdd.ToDto();
    }
}
