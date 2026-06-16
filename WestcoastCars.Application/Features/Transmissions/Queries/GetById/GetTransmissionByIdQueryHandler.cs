using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Transmissions.Queries.GetById;

public class GetTransmissionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTransmissionByIdQuery, NamedObjectDto?>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<NamedObjectDto?> Handle(GetTransmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var transmissionType = await _unitOfWork.TransmissionTypeRepository.GetByIdAsync(request.Id);
        if (transmissionType is null) return null;
        return _mapper.Map<NamedObjectDto>(transmissionType);
    }
}
