using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Stats;

public class GetVehicleStatsByMileageQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetVehicleStatsByMileageQuery, IEnumerable<VehicleStatsByMileageDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<VehicleStatsByMileageDto>> Handle(GetVehicleStatsByMileageQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.VehicleRepository.GetStatsByMileageAsync();
    }
}
