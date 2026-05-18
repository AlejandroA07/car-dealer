namespace WestcoastCars.Application.Models.Blocket;

public class BlocketCarSearchRequest
{
    public string? Query { get; set; }
    public int Page { get; set; } = 1;
    public string? SortOrder { get; set; }
    public string? OrgId { get; set; }
    public string? Locations { get; set; }
    public string? Models { get; set; }
    public int? PriceFrom { get; set; }
    public int? PriceTo { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public int? MilageFrom { get; set; }
    public int? MilageTo { get; set; }
    public string? Colors { get; set; }
    public string? Transmissions { get; set; }
    public string? WheelDrive { get; set; }
    public int? HorsepowerFrom { get; set; }
    public int? HorsepowerTo { get; set; }
}
