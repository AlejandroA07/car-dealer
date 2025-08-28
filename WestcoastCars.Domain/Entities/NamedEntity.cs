namespace WestcoastCars.Domain.Entities;

public abstract class NamedEntity : BaseEntity
{
    public required string Name { get; set; }
}
