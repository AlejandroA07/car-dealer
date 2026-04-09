namespace WestcoastCars.Application.Models.Blocket;

public class BlocketCarSearchRequest
{
    public int Page { get; set; } = 1;
    public string? SortOrder { get; set; }
    public string? OrgId { get; set; }
    public string? Locations { get; set; }
    public string? Models { get; set; }
    public int? PriceFrom { get; set; }
    public int? PriceTo { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
}
