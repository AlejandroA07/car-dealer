
namespace WestcoastCars.Contracts.DTOs;

public class VehicleDetailsDto
{
    public int Id { get; set; }
    public string? RegistrationNumber { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string TransmissionType { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ModelYear { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsSold { get; set; }
    public string? ExternalListingId { get; set; }
    public string? Source { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ImportedAt { get; set; }
    public string? Color { get; set; }
    public string? City { get; set; }
}
