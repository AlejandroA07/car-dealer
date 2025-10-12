using westcoast_cars.web.ViewModels.FuelType;

namespace westcoast_cars.web.Services
{
    public class FuelTypeService : GenericDataService<FuelTypeListViewModel, FuelTypePostViewModel>, IFuelTypeService
    {
        public FuelTypeService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<FuelTypeService> logger)
            : base(httpClientFactory, config, logger, "fueltypes")
        {
        }
    }
}
