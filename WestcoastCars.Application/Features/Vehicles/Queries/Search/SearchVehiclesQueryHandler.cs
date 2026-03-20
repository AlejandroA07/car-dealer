using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Search;

public class SearchVehiclesQueryHandler : IRequestHandler<SearchVehiclesQuery, IEnumerable<VehicleSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchVehiclesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VehicleSummaryDto>> Handle(SearchVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.VehicleRepository.SearchAsync(request.Criteria);
        return _mapper.Map<IEnumerable<VehicleSummaryDto>>(vehicles);
    }
}
