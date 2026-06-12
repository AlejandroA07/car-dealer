using WestcoastCars.Web.ViewModels.CoreAttributes;

namespace WestcoastCars.Web.Services;

public class TransmissionTypeService(IHttpClientFactory httpClientFactory, ILogger<TransmissionTypeService> logger) : GenericDataService<AttributeItemViewModel, AttributePostViewModel>(httpClientFactory, logger, "transmissions"), ITransmissionTypeService
{
}
