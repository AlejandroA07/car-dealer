using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WestcoastCars.Web.ViewModels.Manufacturer
{
    public class ManufacturerPostViewModel : BaseViewModel
    {
        [JsonIgnore]
        public IList<ManufacturerListViewModel> Manufacturers { get; set; } = new List<ManufacturerListViewModel>();
    }
}
