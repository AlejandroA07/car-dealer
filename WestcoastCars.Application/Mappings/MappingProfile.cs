using AutoMapper;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Mappings
{
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
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.ImageUrl) || src.ImageUrl == "no-car.png"
                    ? "/images/no-car.png" 
                    : (src.ImageUrl.StartsWith("/") || src.ImageUrl.StartsWith("http") ? src.ImageUrl : "/images/" + src.ImageUrl)));

            CreateMap<Vehicle, VehicleDetailsDto>()
                .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer.Name))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Manufacturer.Name} {src.Model}"))
                .ForMember(dest => dest.FuelType, opt => opt.MapFrom(src => src.FuelType.Name))
                .ForMember(dest => dest.TransmissionsType, opt => opt.MapFrom(src => src.TransmissionType.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.ImageUrl) || src.ImageUrl == "no-car.png"
                    ? "/images/no-car.png" 
                    : (src.ImageUrl.StartsWith("/") || src.ImageUrl.StartsWith("http") ? src.ImageUrl : "/images/" + src.ImageUrl)));

            CreateMap<ServiceBooking, ServiceBookingSummaryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
