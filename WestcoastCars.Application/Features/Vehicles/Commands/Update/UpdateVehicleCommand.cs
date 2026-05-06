using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Update;

public class UpdateVehicleCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public int ManufacturerId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string ModelYear { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public int FuelTypeId { get; set; }
    public int TransmissionTypeId { get; set; }
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsSold { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
