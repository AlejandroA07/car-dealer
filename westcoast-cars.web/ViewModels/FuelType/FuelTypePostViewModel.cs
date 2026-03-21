using System.Collections.Generic;
using System.Text.Json.Serialization;
using westcoast_cars.web.ViewModels;

namespace westcoast_cars.web.ViewModels.FuelType
{
    public class FuelTypePostViewModel : BaseViewModel
    {
        [JsonIgnore]
        public IList<FuelTypeListViewModel> FuelTypes { get; set; }
    }
}