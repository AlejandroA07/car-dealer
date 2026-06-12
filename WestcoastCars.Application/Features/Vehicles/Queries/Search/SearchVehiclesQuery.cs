using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Search;

public class SearchVehiclesQuery(VehicleSearchDto criteria) : IRequest<PagedResult<VehicleSummaryDto>>
{
    public VehicleSearchDto Criteria { get; set; } = criteria;
}
