using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Transmissions.Queries.GetById;

public class GetTransmissionByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTransmissionByIdQuery, NamedObjectDto?>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<NamedObjectDto?> Handle(GetTransmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var transmissionType = await _unitOfWork.TransmissionTypeRepository.GetByIdAsync(request.Id);
        if (transmissionType is null) return null;
        return transmissionType.ToDto();
    }
}
