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
        }
    }
}
