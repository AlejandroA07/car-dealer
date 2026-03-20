using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Search;

public class SearchVehiclesQuery : IRequest<IEnumerable<VehicleSummaryDto>>
{
    public VehicleSearchDto Criteria { get; set; }

    public SearchVehiclesQuery(VehicleSearchDto criteria)
    {
        Criteria = criteria;
    }
}
