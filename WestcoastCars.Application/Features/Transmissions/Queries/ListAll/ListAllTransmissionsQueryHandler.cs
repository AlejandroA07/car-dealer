using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Transmissions.Queries.ListAll;

public class ListAllTransmissionsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListAllTransmissionsQuery, IEnumerable<NamedObjectDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllTransmissionsQuery request, CancellationToken cancellationToken)
    {
        var transmissionTypes = await _unitOfWork.TransmissionTypeRepository.GetAllAsync();
        return transmissionTypes.Select(t => t.ToDto());
    }
}
