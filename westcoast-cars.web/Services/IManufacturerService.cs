using westcoast_cars.web.ViewModels.Manufacturer;

namespace westcoast_cars.web.Services
{
    public interface IManufacturerService
    {
        Task<IList<ManufacturerListViewModel>> ListAllManufacturersAsync();
        Task<bool> CreateManufacturerAsync(ManufacturerPostViewModel model);
        Task<bool> DeleteManufacturerAsync(int id);
    }
}