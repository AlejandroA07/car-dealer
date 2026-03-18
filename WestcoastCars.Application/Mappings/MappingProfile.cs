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
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl ?? "/images/no-car.png"));

            CreateMap<Vehicle, VehicleDetailsDto>()
                .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer.Name))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Manufacturer.Name} {src.Model}"))
                .ForMember(dest => dest.FuelType, opt => opt.MapFrom(src => src.FuelType.Name))
                .ForMember(dest => dest.TransmissionsType, opt => opt.MapFrom(src => src.TransmissionType.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl ?? "/images/no-car.png"));
        }
    }
}
