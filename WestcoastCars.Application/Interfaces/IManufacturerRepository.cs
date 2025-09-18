
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IManufacturerRepository : IRepository<Manufacturer>
{
    Task<Manufacturer?> FindByNameWithVehiclesAsync(string name);
}

