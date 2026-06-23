using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.GetById;

public class GetVehicleByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetVehicleByIdQuery, VehicleDetailsDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<VehicleDetailsDto> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"Vehicle with ID {request.Id} not found");
        return vehicle.ToDetailsDto();
    }
}
