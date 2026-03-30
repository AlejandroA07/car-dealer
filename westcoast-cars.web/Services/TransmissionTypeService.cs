using westcoast_cars.web.ViewModels.TransmissionType;

namespace westcoast_cars.web.Services
{
    public class TransmissionTypeService : GenericDataService<TransmissionTypeListViewModel, TransmissionTypePostViewModel>, ITransmissionTypeService
    {
        public TransmissionTypeService(IHttpClientFactory httpClientFactory, ILogger<TransmissionTypeService> logger)
            : base(httpClientFactory, logger, "transmissions")
        {
        }
    }
}
