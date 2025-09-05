
using System.ComponentModel.DataAnnotations;

namespace WestcoastCars.Contracts.DTOs
{
    public class NamedObjectDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Namn måste anges")]
        public string Name { get; set; } = string.Empty;
    }
}
