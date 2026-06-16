using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Transmissions.Queries.ListAll;

public class ListAllTransmissionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<ListAllTransmissionsQuery, IEnumerable<NamedObjectDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllTransmissionsQuery request, CancellationToken cancellationToken)
    {
        var transmissionTypes = await _unitOfWork.TransmissionTypeRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<NamedObjectDto>>(transmissionTypes);
    }
}
