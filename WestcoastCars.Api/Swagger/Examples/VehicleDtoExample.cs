using Swashbuckle.AspNetCore.Filters;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Api.Swagger.Examples;

public class VehicleDtoExample : IExamplesProvider<VehicleDto>
{
    public VehicleDto GetExamples()
    {
        return new VehicleDto
        {
            Id = 1,
            RegistrationNumber = "ABC123",
            ManufacturerId = 1,
            Model = "Model S",
            ModelYear = "2023",
            Mileage = 15000,
            FuelTypeId = 1,
            TransmissionTypeId = 1,
            Value = 450000,
            Description = "Electric sedan in excellent condition",
            IsSold = false,
            ImageUrl = "/images/tesla-model-s.jpg"
        };
    }
}
