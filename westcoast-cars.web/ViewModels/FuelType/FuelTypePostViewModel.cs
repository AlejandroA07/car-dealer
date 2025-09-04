using System.Collections.Generic;
using westcoast_cars.web.ViewModels;

namespace westcoast_cars.web.ViewModels.FuelType
{
    public class FuelTypePostViewModel : BaseViewModel
    {
        public IList<FuelTypeListViewModel> FuelTypes { get; set; }
    }
}