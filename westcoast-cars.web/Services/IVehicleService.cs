using westcoast_cars.web.ViewModels.Vehicles;
using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.Services
{
    public interface IVehicleService
    {
        Task<List<VehicleSummaryDto>> ListVehiclesAsync();
        Task<List<VehicleSummaryDto>> ListAllVehiclesAsync();
        Task<VehicleDetailsDto?> GetVehicleByIdAsync(int id);
        Task<bool> DeleteVehicleAsync(int id);
        Task<VehicleBaseViewModel?> GetVehicleForEditAsync(int id);
        Task<bool> UpdateVehicleAsync(int id, VehicleDto vehicle);
        Task<VehicleBaseViewModel?> GetVehicleForCreateAsync();
        Task<bool> CreateVehicleAsync(VehicleBaseViewModel vehicleViewModel);
        Task<List<VehicleSummaryDto>> SearchVehiclesAsync(VehicleSearchDto search);
    }
}
