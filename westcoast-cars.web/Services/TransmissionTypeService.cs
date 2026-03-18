using westcoast_cars.web.ViewModels.TransmissionType;

namespace westcoast_cars.web.Services
{
    public class TransmissionTypeService : GenericDataService<TransmissionTypeListViewModel, TransmissionTypePostViewModel>, ITransmissionTypeService
    {
        public TransmissionTypeService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<TransmissionTypeService> logger)
            : base(httpClientFactory, config, logger, "transmissions")
        {
        }
    }
}
