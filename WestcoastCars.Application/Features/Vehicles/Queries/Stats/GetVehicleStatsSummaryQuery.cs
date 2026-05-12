using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.Stats;

public class GetVehicleStatsSummaryQuery : IRequest<VehicleStatsSummaryDto> { }
