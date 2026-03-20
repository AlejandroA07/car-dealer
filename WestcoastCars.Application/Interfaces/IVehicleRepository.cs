
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> FindByRegistrationNumberAsync(string regNo);
    Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchDto search);
}
