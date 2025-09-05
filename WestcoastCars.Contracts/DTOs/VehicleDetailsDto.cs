
namespace WestcoastCars.Contracts.DTOs
{
    public class VehicleDetailsDto
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public string TransmissionsType { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public int Value { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ModelYear { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
