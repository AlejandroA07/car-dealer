using System.Text.Json;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Mappings;

public static class DomainMappings
{
    public static NamedObjectDto ToDto(this Manufacturer m) =>
        new() { Id = m.Id, Name = m.Name };

    public static NamedObjectDto ToDto(this FuelType f) =>
        new() { Id = f.Id, Name = f.Name };

    public static NamedObjectDto ToDto(this TransmissionType t) =>
        new() { Id = t.Id, Name = t.Name };

    public static ServiceBookingSummaryDto ToDto(this ServiceBooking s) => new()
    {
        Id = s.Id,
        VehicleRegistrationNumber = s.VehicleRegistrationNumber,
        ServiceType = s.ServiceType,
        BookingDate = s.BookingDate,
        TimeSlot = s.TimeSlot.ToString(),
        CustomerName = s.CustomerName,
        CustomerEmail = s.CustomerEmail,
        CustomerPhone = s.CustomerPhone,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt
    };

    public static VehicleSummaryDto ToSummaryDto(this Vehicle v) => new()
    {
        Id = v.Id,
        Name = $"{v.Manufacturer.Name} {v.Model}",
        Manufacturer = v.Manufacturer.Name,
        Model = v.Model,
        ModelYear = v.ModelYear,
        ImageUrl = NormalizeImageUrl(v.ImageUrl),
        IsSold = v.IsSold,
        Price = v.Price,
        Color = v.Color,
        City = v.City,
        Source = v.Source,
        PublishedAt = v.PublishedAt
    };

    public static VehicleDetailsDto ToDetailsDto(this Vehicle v) => new()
    {
        Id = v.Id,
        RegistrationNumber = v.RegistrationNumber,
        FuelType = v.FuelType.Name,
        TransmissionType = v.TransmissionType.Name,
        Mileage = v.Mileage,
        Price = (int)v.Price,
        Description = v.Description,
        Name = $"{v.Manufacturer.Name} {v.Model}",
        Manufacturer = v.Manufacturer.Name,
        Model = v.Model,
        ModelYear = v.ModelYear,
        ImageUrl = NormalizeImageUrl(v.ImageUrl),
        ImageUrls = BuildImageUrls(v.ImageUrl, v.GalleryUrls),
        IsSold = v.IsSold,
        ExternalListingId = v.ExternalListingId,
        Source = v.Source,
        SourceUrl = v.SourceUrl,
        PublishedAt = v.PublishedAt,
        ImportedAt = v.ImportedAt,
        Color = v.Color,
        WheelDrive = v.WheelDrive,
        Horsepower = v.Horsepower,
        BodyType = v.BodyType,
        Doors = v.Doors,
        EngineVolume = v.EngineVolume,
        City = v.City,
        Address = v.Address,
        Equipment = DeserializeEquipment(v.Equipment),
        Seats = v.Seats,
        MaxTrailerWeight = v.MaxTrailerWeight,
        OwnerCount = v.OwnerCount,
        LastInspectionDate = v.LastInspectionDate,
        NextInspectionDate = v.NextInspectionDate
    };

    private static string NormalizeImageUrl(string? imageUrl) =>
        string.IsNullOrEmpty(imageUrl) || imageUrl == "no-car.png"
            ? "/images/no-car.png"
            : imageUrl.StartsWith('/') || imageUrl.StartsWith("http")
                ? imageUrl
                : "/images/" + imageUrl;

    private static List<string> BuildImageUrls(string? imageUrl, string? galleryJson)
    {
        if (!string.IsNullOrWhiteSpace(galleryJson))
        {
            try
            {
                var gallery = JsonSerializer.Deserialize<List<string>>(galleryJson);
                if (gallery is { Count: > 0 })
                    return [.. gallery.Select(NormalizeImageUrl)];
            }
            catch { }
        }
        return [NormalizeImageUrl(imageUrl)];
    }

    private static List<string> DeserializeEquipment(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
