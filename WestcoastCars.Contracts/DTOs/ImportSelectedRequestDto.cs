namespace WestcoastCars.Contracts.DTOs;

public class ImportSelectedRequestDto
{
    public List<string> ExternalListingIds { get; set; } = [];
    public Dictionary<string, string> ImageUrlsById { get; set; } = [];
}
