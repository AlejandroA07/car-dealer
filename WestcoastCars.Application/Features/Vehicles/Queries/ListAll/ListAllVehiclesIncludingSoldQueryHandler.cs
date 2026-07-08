using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.ListAll;

public class ListAllVehiclesIncludingSoldQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListAllVehiclesIncludingSoldQuery, PagedResult<VehicleSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<VehicleSummaryDto>> Handle(ListAllVehiclesIncludingSoldQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.VehicleRepository.GetAllPagedAsync(new PagedQueryDto
        {
            Page = request.Page,
            PageSize = request.PageSize
        });

        return new PagedResult<VehicleSummaryDto>
        {
            Items = [.. vehicles.Items.Select(v => v.ToSummaryDto())],
            TotalCount = vehicles.TotalCount,
            Page = vehicles.Page,
            PageSize = vehicles.PageSize
        };
    }
}
