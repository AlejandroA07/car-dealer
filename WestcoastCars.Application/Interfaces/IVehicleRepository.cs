
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle?> FindByRegistrationNumberAsync(string regNo);
    Task<IReadOnlyDictionary<string, int>> GetBlocketVehicleIndexAsync();
    Task<List<Vehicle>> GetByIdsAsync(IReadOnlyCollection<int> ids);
    Task<List<Vehicle>> GetByExternalIdsAsync(IReadOnlyCollection<string> externalIds);

    Task<PagedResult<Vehicle>> GetAllPagedAsync(PagedQueryDto pagination);
    Task<PagedResult<Vehicle>> GetUnsoldAsync(PagedQueryDto pagination);
    Task<PagedResult<Vehicle>> SearchAsync(VehicleSearchDto search);
    Task<IEnumerable<VehicleStatsByModelDto>> GetStatsByModelAsync();
    Task<IEnumerable<VehicleStatsByMileageDto>> GetStatsByMileageAsync();
    Task<VehicleStatsSummaryDto> GetStatsSummaryAsync();
    Task<int> DeleteAllAsync(CancellationToken cancellationToken = default);
    Task<int> BulkDeleteAsync(string? make, string? model, bool? isSold, int? minMileage, int? maxMileage, CancellationToken cancellationToken = default);
    Task<int> PurgeSourceRemovedAsync(CancellationToken cancellationToken = default);
}
