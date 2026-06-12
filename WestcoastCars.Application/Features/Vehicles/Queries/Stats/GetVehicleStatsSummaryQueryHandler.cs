using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Stats;

public class GetVehicleStatsSummaryQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetVehicleStatsSummaryQuery, VehicleStatsSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<VehicleStatsSummaryDto> Handle(GetVehicleStatsSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.VehicleRepository.GetStatsSummaryAsync();
    }
}
