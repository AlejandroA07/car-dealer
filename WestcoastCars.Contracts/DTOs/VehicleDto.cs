using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.DTOs;

/// <summary>
/// Represents a detailed view of a vehicle.
/// </summary>
public class VehicleDto
{
    /// <summary>
    /// Unique identifier for the vehicle.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Vehicle registration number (plate).
    /// </summary>
    [Required(ErrorMessage = "RegistrationNumber måste anges")]
    [MaxLength(10)]
    public string RegistrationNumber { get; set; } = string.Empty;

    /// <summary>
    /// ID of the manufacturer (Make).
    /// </summary>
    [Required(ErrorMessage = "Tillverkare måste anges")]
    public int ManufacturerId { get; set; }

    /// <summary>
    /// Model name (e.g., "Model S", "Golf").
    /// </summary>
    [Required(ErrorMessage = "Bilmodell måste anges")]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Production year or model year.
    /// </summary>
    [Required(ErrorMessage = "Årsmodell måste anges")]
    public int ModelYear { get; set; }

    /// <summary>
    /// Current mileage in kilometers.
    /// </summary>
    [Required(ErrorMessage = "Antal körda km måste anges")]
    public int Mileage { get; set; }

    /// <summary>
    /// ID of the fuel type.
    /// </summary>
    [Required(ErrorMessage = "Bränsletyp måste anges")]
    public int FuelTypeId { get; set; }

    /// <summary>
    /// ID of the transmission type.
    /// </summary>
    [Required(ErrorMessage = "Typ av växellåda måste anges")]
    public int TransmissionTypeId { get; set; }

    /// <summary>
    /// Selling price of the vehicle.
    /// </summary>
    [Required(ErrorMessage = "Pris på bilen måste anges")]
    public int Price { get; set; }

    /// <summary>
    /// Detailed description of the vehicle's condition and features.
    /// </summary>
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the vehicle has been sold.
    /// </summary>
    public bool IsSold { get; set; } = false;

    /// <summary>
    /// Relative path or URL to the vehicle's primary image.
    /// </summary>
    [MaxLength(500)]
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

    /// <summary>One item per line.</summary>
    public string? Equipment { get; set; }

    /// <summary>One URL per line.</summary>
    public string? GalleryUrls { get; set; }
}
