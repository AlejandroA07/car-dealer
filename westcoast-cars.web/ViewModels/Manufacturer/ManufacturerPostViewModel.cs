using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace westcoast_cars.web.ViewModels.Manufacturer
{
    public class ManufacturerPostViewModel : BaseViewModel
    {
        [JsonIgnore]
        public IList<ManufacturerListViewModel> Manufacturers { get; set; } = new List<ManufacturerListViewModel>();
    }
}
