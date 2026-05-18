using WestcoastCars.Web.ViewModels.CoreAttributes;

namespace WestcoastCars.Web.Services;

public class ManufacturerService : GenericDataService<AttributeItemViewModel, AttributePostViewModel>, IManufacturerService
{
    public ManufacturerService(IHttpClientFactory httpClientFactory, ILogger<ManufacturerService> logger)
        : base(httpClientFactory, logger, "manufacturers")
    {
    }
}
