namespace WestcoastCars.Contracts.DTOs;

public class BlocketPreviewDto
{
    public string ExternalListingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public int? MileageKm { get; set; }
    public int? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Location { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? SourceUrl { get; set; }
    public string? Transmission { get; set; }
    public string? Fuel { get; set; }
    public bool IsAlreadyImported { get; set; }
}
