
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IFuelTypeRepository : IRepository<FuelType>
{
    Task<FuelType?> FindByNameWithVehiclesAsync(string name);
}

