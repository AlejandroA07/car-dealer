using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.RefreshInventoryFromBlocket;

public class RefreshInventoryFromBlocketCommand : IRequest<RefreshInventoryFromBlocketResult>
{
    public int Limit { get; set; } = 50;
    public string? OrgId { get; set; }
    public string? Locations { get; set; }
    public string? Models { get; set; }
}
