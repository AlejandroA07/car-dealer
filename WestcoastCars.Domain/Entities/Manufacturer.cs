namespace WestcoastCars.Domain.Entities;

public class Manufacturer : NamedEntity
{
    // Navigation property. I can occur per several cars, That's what this line says to ef 
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
