using System.Collections.Generic;
using System.Text.Json.Serialization;
using WestcoastCars.Web.ViewModels;

namespace WestcoastCars.Web.ViewModels.FuelType
{
    public class FuelTypePostViewModel : BaseViewModel
    {
        [JsonIgnore]
        public IList<FuelTypeListViewModel> FuelTypes { get; set; } = new List<FuelTypeListViewModel>();
    }
}
