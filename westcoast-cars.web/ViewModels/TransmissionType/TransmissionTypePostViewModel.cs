using System.Collections.Generic;
using System.Text.Json.Serialization;
using westcoast_cars.web.ViewModels;

namespace westcoast_cars.web.ViewModels.TransmissionType
{
    public class TransmissionTypePostViewModel : BaseViewModel
    {
        [JsonIgnore]
        public IList<TransmissionTypeListViewModel> TransmissionTypes { get; set; }
    }
}