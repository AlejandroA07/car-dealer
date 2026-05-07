using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.ListAll;

public class ListAllVehiclesQuery : IRequest<PagedResult<VehicleSummaryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
