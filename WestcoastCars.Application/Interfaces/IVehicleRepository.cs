
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> FindByRegistrationNumberAsync(string regNo);
    Task<IEnumerable<Vehicle>> GetAllForReplacementAsync();
    Task<PagedResult<Vehicle>> GetAllPagedAsync(PagedQueryDto pagination);
    Task<PagedResult<Vehicle>> GetUnsoldAsync(PagedQueryDto pagination);
    Task<PagedResult<Vehicle>> SearchAsync(VehicleSearchDto search);
}
