using System;
using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Domain.Entities
{
    public class ServiceBooking : BaseEntity
    {
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
