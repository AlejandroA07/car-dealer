using WestcoastCars.Web.ViewModels.Manufacturer;

namespace WestcoastCars.Web.Services;

public class ManufacturerService : GenericDataService<ManufacturerListViewModel, ManufacturerPostViewModel>, IManufacturerService
{
    public ManufacturerService(IHttpClientFactory httpClientFactory, ILogger<ManufacturerService> logger)
        : base(httpClientFactory, logger, "manufacturers")
    {
    }
}
