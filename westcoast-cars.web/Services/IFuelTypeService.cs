using westcoast_cars.web.ViewModels.FuelType;

namespace westcoast_cars.web.Services
{
    public interface IFuelTypeService
    {
        Task<IList<FuelTypeListViewModel>> ListAllFuelTypesAsync();
        Task<bool> CreateFuelTypeAsync(FuelTypePostViewModel model);
        Task<bool> DeleteFuelTypeAsync(int id);
    }
}