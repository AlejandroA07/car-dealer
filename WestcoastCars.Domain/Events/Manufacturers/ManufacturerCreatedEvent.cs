using WestcoastCars.Domain.Common;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Domain.Events.Manufacturers
{
    public class ManufacturerCreatedEvent : DomainEvent
    {
        public Manufacturer Manufacturer { get; }

        public ManufacturerCreatedEvent(Manufacturer manufacturer)
        {
            Manufacturer = manufacturer;
        }
    }
}
