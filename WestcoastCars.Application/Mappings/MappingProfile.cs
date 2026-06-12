using System.Text.Json;
using AutoMapper;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Manufacturer, NamedObjectDto>().ReverseMap();
        CreateMap<FuelType, NamedObjectDto>().ReverseMap();
        CreateMap<TransmissionType, NamedObjectDto>().ReverseMap();

        CreateMap<Vehicle, VehicleSummaryDto>()
            .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer.Name))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Manufacturer.Name} {src.Model}"))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => NormalizeImageUrl(src.ImageUrl)));

        CreateMap<Vehicle, VehicleDetailsDto>()
            .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer.Name))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Manufacturer.Name} {src.Model}"))
            .ForMember(dest => dest.FuelType, opt => opt.MapFrom(src => src.FuelType.Name))
            .ForMember(dest => dest.TransmissionType, opt => opt.MapFrom(src => src.TransmissionType.Name))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => NormalizeImageUrl(src.ImageUrl)))
            .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => DeserializeEquipment(src.Equipment)))
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => BuildImageUrls(src.ImageUrl, src.GalleryUrls)));

        CreateMap<ServiceBooking, ServiceBookingSummaryDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TimeSlot, opt => opt.MapFrom(src => src.TimeSlot.ToString()));
    }

    private static List<string> DeserializeEquipment(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }

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

    private static string NormalizeImageUrl(string? imageUrl) =>
        string.IsNullOrEmpty(imageUrl) || imageUrl == "no-car.png"
            ? "/images/no-car.png"
            : imageUrl.StartsWith("/") || imageUrl.StartsWith("http")
                ? imageUrl
                : "/images/" + imageUrl;
}
