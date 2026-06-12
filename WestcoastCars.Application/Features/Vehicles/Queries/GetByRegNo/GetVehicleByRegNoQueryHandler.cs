using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.GetByRegNo;

public class GetVehicleByRegNoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetVehicleByRegNoQuery, VehicleDetailsDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<VehicleDetailsDto> Handle(GetVehicleByRegNoQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.VehicleRepository.FindByRegistrationNumberAsync(request.RegistrationNumber) ?? throw new NotFoundException($"Vehicle with registration number {request.RegistrationNumber} not found");
        return _mapper.Map<VehicleDetailsDto>(vehicle);
    }
}
