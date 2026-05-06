using WestcoastCars.Web.ViewModels.FuelType;

namespace WestcoastCars.Web.Services;

public class FuelTypeService : GenericDataService<FuelTypeListViewModel, FuelTypePostViewModel>, IFuelTypeService
{
    public FuelTypeService(IHttpClientFactory httpClientFactory, ILogger<FuelTypeService> logger)
        : base(httpClientFactory, logger, "fueltypes")
    {
    }
}
