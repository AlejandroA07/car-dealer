namespace WestcoastCars.Contracts.DTOs;

public class ImportSelectedResult
{
    public int TotalSelected { get; set; }
    public int TotalAdded { get; set; }
    public int TotalUpdated { get; set; }
    public int TotalSkipped { get; set; }
}
