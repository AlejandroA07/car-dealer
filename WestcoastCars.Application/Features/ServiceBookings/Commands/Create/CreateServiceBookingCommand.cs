using MediatR;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Create;

public class CreateServiceBookingCommand : IRequest<int>
{
    public string VehicleRegistrationNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
