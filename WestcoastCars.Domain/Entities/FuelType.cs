namespace WestcoastCars.Domain.Entities;

public class FuelType : NamedEntity
{
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
