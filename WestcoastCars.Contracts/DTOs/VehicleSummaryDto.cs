
namespace WestcoastCars.Contracts.DTOs;

public class VehicleSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ModelYear { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsSold { get; set; }
    public decimal Price { get; set; }
    public string? Color { get; set; }
    public string? City { get; set; }
    public string? Source { get; set; }
    public DateTime? PublishedAt { get; set; }
}
