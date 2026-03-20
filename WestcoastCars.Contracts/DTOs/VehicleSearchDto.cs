namespace WestcoastCars.Contracts.DTOs;

public class VehicleSearchDto
{
    public string? Make { get; set; } // Manufacturer Name
    public string? Model { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public bool? IsSold { get; set; }
}

