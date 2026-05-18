using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.RefreshInventoryFromBlocket;

public class RefreshInventoryFromBlocketCommand : IRequest<RefreshInventoryFromBlocketResult>
{
    public int Limit { get; set; } = 50;
    public string? Query { get; set; }
    public string? SortOrder { get; set; }
    public string? OrgId { get; set; }
    public string? Locations { get; set; }
    public string? Manufacturers { get; set; }
    public int? PriceFrom { get; set; }
    public int? PriceTo { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public int? MinMileage { get; set; }
    public int? MaxMileage { get; set; }
    public string? Colors { get; set; }
    public string? TransmissionFilter { get; set; }
    public string? WheelDrive { get; set; }
    public int? HorsepowerFrom { get; set; }
    public int? HorsepowerTo { get; set; }
    public string? FuelTypeFilter { get; set; }
}
