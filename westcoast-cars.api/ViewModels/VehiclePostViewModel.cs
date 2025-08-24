using System.ComponentModel.DataAnnotations;

namespace westcoast_cars.api.ViewModels
{
    public class VehiclePostViewModel : VehicleBaseViewModel
    {
        [Required(ErrorMessage = "RegistrationNumber måste anges")]
        public string RegistrationNumber { get; set; }
    }
}