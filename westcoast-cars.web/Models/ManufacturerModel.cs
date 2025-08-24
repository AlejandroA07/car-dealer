namespace westcoast_cars.web.Models
{
    public class ManufacturerModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Navigation property. I can occur per several cars, That's what this line says to ef 
        public ICollection<VehicleModel> Vehicles { get; set; }
    }
}