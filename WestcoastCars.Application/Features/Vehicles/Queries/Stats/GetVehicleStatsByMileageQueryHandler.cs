using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Stats;

public class GetVehicleStatsByMileageQueryHandler : IRequestHandler<GetVehicleStatsByMileageQuery, IEnumerable<VehicleStatsByMileageDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVehicleStatsByMileageQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<VehicleStatsByMileageDto>> Handle(GetVehicleStatsByMileageQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.VehicleRepository.GetStatsByMileageAsync();
    }
}
