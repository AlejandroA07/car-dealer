
namespace WestcoastCars.Contracts.DTOs
{
    public class VehicleSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ModelYear { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsSold { get; set; }
        public decimal Value { get; set; }
    }
}
