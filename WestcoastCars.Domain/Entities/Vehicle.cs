using System.ComponentModel.DataAnnotations.Schema;

namespace WestcoastCars.Domain.Entities;

public class Vehicle : BaseEntity
{
    public required string RegistrationNumber { get; set; }
    public required string Model { get; set; }
    public required string ModelYear { get; set; }
    public int Mileage { get; set; }
    public required string ImageUrl { get; set; }
    public int Value { get; set; }
    public required string Description { get; set; }
    public bool IsSold { get; set; }
    // Navigation property...
    public int MakeId { get; set; }
    // Composition...
    [ForeignKey("MakeId")]
    public required Manufacturer Manufacturer { get; set; }

    public int FuelTypeId { get; set; }
    [ForeignKey("FuelTypeId")]
    public required FuelType FuelType { get; set; }

    public int TransmissionTypeId { get; set; }
    [ForeignKey("TransmissionTypeId")]
    public required TransmissionType TransmissionType { get; set; }
}
