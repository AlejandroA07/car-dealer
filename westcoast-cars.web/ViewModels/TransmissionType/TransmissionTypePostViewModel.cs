using System.Collections.Generic;
using westcoast_cars.web.ViewModels;

namespace westcoast_cars.web.ViewModels.TransmissionType
{
    public class TransmissionTypePostViewModel : BaseViewModel
    {
        public IList<TransmissionTypeListViewModel> TransmissionTypes { get; set; }
    }
}