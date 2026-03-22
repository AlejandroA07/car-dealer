using System;

namespace WestcoastCars.Contracts.DTOs
{
    public class ServiceBookingSummaryDto
    {
        public int Id { get; set; }
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
