using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Transmissions.Queries.GetById;

public class GetTransmissionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTransmissionByIdQuery, NamedObjectDto?>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<NamedObjectDto?> Handle(GetTransmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.TransmissionTypeRepository ?? throw new InvalidOperationException("Repository for TransmissionType is not available.");
        var transmissionType = await repository.GetByIdAsync(request.Id);
        if (transmissionType is null) return null;

        var result = _mapper.Map<NamedObjectDto>(transmissionType!);
        return result;
    }
}
