using WestcoastCars.Web.ViewModels.CoreAttributes;

namespace WestcoastCars.Web.Services;

public class TransmissionTypeService : GenericDataService<AttributeItemViewModel, AttributePostViewModel>, ITransmissionTypeService
{
    public TransmissionTypeService(IHttpClientFactory httpClientFactory, ILogger<TransmissionTypeService> logger)
        : base(httpClientFactory, logger, "transmissions")
    {
    }
}
