using WestcoastCars.Web.ViewModels.CoreAttributes;

namespace WestcoastCars.Web.Services;

public class ManufacturerService(IHttpClientFactory httpClientFactory, ILogger<ManufacturerService> logger) : GenericDataService<AttributeItemViewModel, AttributePostViewModel>(httpClientFactory, logger, "manufacturers"), IManufacturerService
{
}
