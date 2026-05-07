using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Create;

public class CreateTransmissionCommandHandler : IRequestHandler<CreateTransmissionCommand, NamedObjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTransmissionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<NamedObjectDto> Handle(CreateTransmissionCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.TransmissionTypeRepository;
        if (repository is null) throw new InvalidOperationException("Repository for TransmissionType is not available.");

        var transmissionTypeToAdd = new TransmissionType { Name = request.Name };
        await repository.AddAsync(transmissionTypeToAdd);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create transmission type");
        return _mapper.Map<NamedObjectDto>(transmissionTypeToAdd);
    }
}
