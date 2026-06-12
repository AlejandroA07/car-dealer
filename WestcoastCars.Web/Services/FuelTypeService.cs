using WestcoastCars.Web.ViewModels.CoreAttributes;

namespace WestcoastCars.Web.Services;

public class FuelTypeService(IHttpClientFactory httpClientFactory, ILogger<FuelTypeService> logger) : GenericDataService<AttributeItemViewModel, AttributePostViewModel>(httpClientFactory, logger, "fueltypes"), IFuelTypeService
{
}
