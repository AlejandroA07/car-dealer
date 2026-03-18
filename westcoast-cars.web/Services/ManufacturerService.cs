using westcoast_cars.web.ViewModels.Manufacturer;

namespace westcoast_cars.web.Services
{
    public class ManufacturerService : GenericDataService<ManufacturerListViewModel, ManufacturerPostViewModel>, IManufacturerService
    {
        public ManufacturerService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ManufacturerService> logger)
            : base(httpClientFactory, config, logger, "manufacturers")
        {
        }
    }
}
