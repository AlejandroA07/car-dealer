using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.ListAll;

public class ListAllVehiclesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListAllVehiclesQuery, PagedResult<VehicleSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<VehicleSummaryDto>> Handle(ListAllVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.VehicleRepository.GetUnsoldAsync(new PagedQueryDto
        {
            Page = request.Page,
            PageSize = request.PageSize
        });

        return new PagedResult<VehicleSummaryDto>
        {
            Items = vehicles.Items.Select(v => v.ToSummaryDto()).ToList(),
            TotalCount = vehicles.TotalCount,
            Page = vehicles.Page,
            PageSize = vehicles.PageSize
        };
    }
}
