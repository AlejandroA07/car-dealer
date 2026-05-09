namespace WestcoastCars.Domain.Entities;

public class TransmissionType : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
