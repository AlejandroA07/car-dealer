using WestcoastCars.Web.ViewModels.Vehicles;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.Services;

public interface IVehicleService
{
    Task<PagedResult<VehicleSummaryDto>> ListVehiclesAsync(int page = 1, int pageSize = 20);
    Task<List<VehicleSummaryDto>> ListAllVehiclesAsync();
    Task<VehicleDetailsDto?> GetVehicleByIdAsync(int id);
    Task<bool> DeleteVehicleAsync(int id);
    Task<VehicleBaseViewModel?> GetVehicleForEditAsync(int id);
    Task<bool> UpdateVehicleAsync(int id, VehicleDto vehicle);
    Task<VehicleBaseViewModel> GetVehicleForCreateAsync();
    Task<bool> CreateVehicleAsync(VehicleBaseViewModel vehicleViewModel);
    Task<PagedResult<VehicleSummaryDto>> SearchVehiclesAsync(VehicleSearchDto search);
    Task<BlocketSyncViewModel> SyncBlocketAsync(BlocketSyncViewModel model);
}
