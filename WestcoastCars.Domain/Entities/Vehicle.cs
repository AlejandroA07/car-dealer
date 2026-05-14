using System.ComponentModel.DataAnnotations.Schema;

namespace WestcoastCars.Domain.Entities;

public class Vehicle : BaseEntity
{
    public required string RegistrationNumber { get; set; }
    public required string Model { get; set; }
    public int ModelYear { get; set; }
    public int Mileage { get; set; }
    public required string ImageUrl { get; set; }
    public int Price { get; set; }
    public required string Description { get; set; }
    public bool IsSold { get; private set; }

    public void MarkAsSold()
    {
        if (IsSold) throw new InvalidOperationException("Vehicle is already sold.");
        IsSold = true;
    }

    public void MarkAsAvailable()
    {
        IsSold = false;
    }

    public string SourceStatus { get; private set; } = "Active";
    public DateTime? SourceRemovedAt { get; private set; }

    public void MarkAsSourceRemoved(DateTime removedAtUtc)
    {
        SourceStatus = "SourceRemoved";
        SourceRemovedAt = removedAtUtc;
    }
    public string? ExternalListingId { get; set; }
    public string? Source { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ImportedAt { get; set; }
    public string? Color { get; set; }
    public string? City { get; set; }
    public string? Equipment { get; set; }
    public int ManufacturerId { get; set; }
    [ForeignKey("ManufacturerId")]
    public required Manufacturer Manufacturer { get; set; }

    public int FuelTypeId { get; set; }
    [ForeignKey("FuelTypeId")]
    public required FuelType FuelType { get; set; }

    public int TransmissionTypeId { get; set; }
    [ForeignKey("TransmissionTypeId")]
    public required TransmissionType TransmissionType { get; set; }

    public ICollection<ServiceBooking> ServiceBookings { get; set; } = new List<ServiceBooking>();
}
