using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Stats;

public class GetVehicleStatsByModelQueryHandler : IRequestHandler<GetVehicleStatsByModelQuery, IEnumerable<VehicleStatsByModelDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetVehicleStatsByModelQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<VehicleStatsByModelDto>> Handle(GetVehicleStatsByModelQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.VehicleRepository.GetStatsByModelAsync();
    }
}
