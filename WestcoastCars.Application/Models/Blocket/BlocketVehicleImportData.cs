namespace WestcoastCars.Application.Models.Blocket;

public class BlocketVehicleImportData
{
    public string? RegistrationNumber { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? ModelYear { get; set; }
    public int Mileage { get; set; }
    public string ImageUrl { get; set; } = "/images/no-car.png";
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string FuelType { get; set; } = "Unknown";
    public string TransmissionType { get; set; } = "Unknown";
    public string ExternalListingId { get; set; } = string.Empty;
    public string Source { get; set; } = "Blocket";
    public string? SourceUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime ImportedAt { get; set; }
    public string? Color { get; set; }
    public string? WheelDrive { get; set; }
    public int? Horsepower { get; set; }
    public string? BodyType { get; set; }
    public int? Doors { get; set; }
    public string? EngineVolume { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public List<string> Equipment { get; set; } = [];
    public List<string> GalleryUrls { get; set; } = [];
    public int? Seats { get; set; }
    public int? MaxTrailerWeight { get; set; }
    public int? OwnerCount { get; set; }
    public DateOnly? LastInspectionDate { get; set; }
    public DateOnly? NextInspectionDate { get; set; }
}
