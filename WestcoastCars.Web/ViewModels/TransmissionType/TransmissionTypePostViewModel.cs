using System.Collections.Generic;
using System.Text.Json.Serialization;
using WestcoastCars.Web.ViewModels;

namespace WestcoastCars.Web.ViewModels.TransmissionType
{
    public class TransmissionTypePostViewModel : BaseViewModel
    {
        [JsonIgnore]
        public IList<TransmissionTypeListViewModel> TransmissionTypes { get; set; } = new List<TransmissionTypeListViewModel>();
    }
}
