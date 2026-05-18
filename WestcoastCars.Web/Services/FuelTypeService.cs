using WestcoastCars.Web.ViewModels.CoreAttributes;

namespace WestcoastCars.Web.Services;

public class FuelTypeService : GenericDataService<AttributeItemViewModel, AttributePostViewModel>, IFuelTypeService
{
    public FuelTypeService(IHttpClientFactory httpClientFactory, ILogger<FuelTypeService> logger)
        : base(httpClientFactory, logger, "fueltypes")
    {
    }
}
