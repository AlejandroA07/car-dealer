using System.Linq.Expressions;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IFuelTypeRepository
{
    Task<FuelType?> FindByIdAsync(int id);
    Task<IReadOnlyList<FuelType>> ListAllAsync();
    Task<IReadOnlyList<FuelType>> ListAsync(Expression<Func<FuelType, bool>> expression);
    Task<FuelType?> FindByNameWithVehiclesAsync(string name);
    Task AddAsync(FuelType fuelType);
}
