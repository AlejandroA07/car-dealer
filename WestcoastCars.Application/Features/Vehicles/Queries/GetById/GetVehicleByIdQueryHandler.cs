using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.GetById;

public class GetVehicleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetVehicleByIdQuery, VehicleDetailsDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<VehicleDetailsDto> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"Vehicle with ID {request.Id} not found");
        return _mapper.Map<VehicleDetailsDto>(vehicle);
    }
}
