using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Search;

public class SearchVehiclesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<SearchVehiclesQuery, PagedResult<VehicleSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<VehicleSummaryDto>> Handle(SearchVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.VehicleRepository.SearchAsync(request.Criteria);
        return new PagedResult<VehicleSummaryDto>
        {
            Items = vehicles.Items.Select(v => v.ToSummaryDto()).ToList(),
            TotalCount = vehicles.TotalCount,
            Page = vehicles.Page,
            PageSize = vehicles.PageSize
        };
    }
}
