using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Domain.Entities;

public class ServiceBooking : BaseEntity
{
    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm a booking with status '{Status}'.");
        Status = BookingStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed booking.");
        Status = BookingStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException($"Cannot complete a booking with status '{Status}'.");
        Status = BookingStatus.Completed;
    }
}
