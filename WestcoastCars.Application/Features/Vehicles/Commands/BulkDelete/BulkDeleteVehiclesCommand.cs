using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.BulkDelete;

public class BulkDeleteVehiclesCommand : IRequest<BulkDeleteVehiclesResult>
{
    public string? Model { get; set; }
    public bool? IsSold { get; set; }
    public int? MinMileage { get; set; }
    public int? MaxMileage { get; set; }
}
