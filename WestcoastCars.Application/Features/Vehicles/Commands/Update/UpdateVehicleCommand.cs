using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Update;

public class UpdateVehicleCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public int ManufacturerId { get; set; }
    public string Model { get; set; } = string.Empty;
    public int ModelYear { get; set; }
    public int Mileage { get; set; }
    public int FuelTypeId { get; set; }
    public int TransmissionTypeId { get; set; }
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsSold { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? WheelDrive { get; set; }
    public int? Horsepower { get; set; }
    public string? BodyType { get; set; }
    public int? Doors { get; set; }
    public string? EngineVolume { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? Seats { get; set; }
    public int? MaxTrailerWeight { get; set; }
    public int? OwnerCount { get; set; }
    public DateOnly? LastInspectionDate { get; set; }
    public DateOnly? NextInspectionDate { get; set; }
    public string? Equipment { get; set; }
    public string? GalleryUrls { get; set; }
}
