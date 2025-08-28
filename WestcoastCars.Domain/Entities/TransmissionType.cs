namespace WestcoastCars.Domain.Entities;

public class TransmissionType : NamedEntity
{
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
