using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.GetByRegNo;

public class GetVehicleByRegNoQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetVehicleByRegNoQuery, VehicleDetailsDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<VehicleDetailsDto> Handle(GetVehicleByRegNoQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.VehicleRepository.FindByRegistrationNumberAsync(request.RegistrationNumber) ?? throw new NotFoundException($"Vehicle with registration number {request.RegistrationNumber} not found");
        return vehicle.ToDetailsDto();
    }
}
