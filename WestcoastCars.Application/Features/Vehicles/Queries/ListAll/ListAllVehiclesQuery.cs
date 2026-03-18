using MediatR;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;

namespace WestcoastCars.Application.Features.Vehicles.Queries.ListAll
{
    public class ListAllVehiclesQuery : IRequest<IEnumerable<VehicleSummaryDto>>
    {
    }
}
