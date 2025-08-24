namespace westcoast_cars.api.Entities
{
    public class Manufacturer : BaseEntity
    {
        // Navigation property. I can occur per several cars, That's what this line says to ef 
        public ICollection<Vehicle> Vehicles { get; set; }
    }
}