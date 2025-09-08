using westcoast_cars.web.ViewModels.TransmissionType;

namespace westcoast_cars.web.Services
{
    public interface ITransmissionTypeService
    {
        Task<IList<TransmissionTypeListViewModel>> ListAllTransmissionTypesAsync();
        Task<bool> CreateTransmissionTypeAsync(TransmissionTypePostViewModel model);
        Task<bool> DeleteTransmissionTypeAsync(int id);
    }
}
