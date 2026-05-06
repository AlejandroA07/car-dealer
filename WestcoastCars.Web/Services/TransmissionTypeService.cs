using WestcoastCars.Web.ViewModels.TransmissionType;

namespace WestcoastCars.Web.Services;

public class TransmissionTypeService : GenericDataService<TransmissionTypeListViewModel, TransmissionTypePostViewModel>, ITransmissionTypeService
{
    public TransmissionTypeService(IHttpClientFactory httpClientFactory, ILogger<TransmissionTypeService> logger)
        : base(httpClientFactory, logger, "transmissions")
    {
    }
}
