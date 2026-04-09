namespace WestcoastCars.Application.Models.Blocket;

public class BlocketVehicleImportData
{
    public string? RegistrationNumber { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ModelYear { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string ImageUrl { get; set; } = "/images/no-car.png";
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public string FuelType { get; set; } = "Unknown";
    public string TransmissionType { get; set; } = "Unknown";
    public string ExternalListingId { get; set; } = string.Empty;
    public string Source { get; set; } = "Blocket";
    public string? SourceUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime ImportedAt { get; set; }
    public string? Color { get; set; }
    public string? City { get; set; }
}
