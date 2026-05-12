
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> FindByRegistrationNumberAsync(string regNo);
    Task<IEnumerable<Vehicle>> GetAllImportedFromBlocketAsync();
    Task<IEnumerable<Vehicle>> GetAllSourceRemovedFromBlocketAsync();
    Task<PagedResult<Vehicle>> GetAllPagedAsync(PagedQueryDto pagination);
    Task<PagedResult<Vehicle>> GetUnsoldAsync(PagedQueryDto pagination);
    Task<PagedResult<Vehicle>> SearchAsync(VehicleSearchDto search);
    Task<IEnumerable<VehicleStatsByModelDto>> GetStatsByModelAsync();
    Task<IEnumerable<VehicleStatsByMileageDto>> GetStatsByMileageAsync();
    Task<VehicleStatsSummaryDto> GetStatsSummaryAsync();
    Task<IReadOnlyList<Vehicle>> GetForBulkDeleteAsync(string? model, bool? isSold, int? minMileage, int? maxMileage);
    Task<IReadOnlyList<Vehicle>> GetAllForDeleteAsync();
}
