using MediatR;
using WestcoastCars.Application.Models.Blocket;

namespace WestcoastCars.Application.Features.Vehicles.Commands.SyncBlocket;

public class SyncBlocketVehiclesCommand : IRequest<SyncBlocketVehiclesResult>
{
    public int Limit { get; set; } = 50;
    public string? OrgId { get; set; }
    public string? Locations { get; set; }
    public string? Models { get; set; }
}
