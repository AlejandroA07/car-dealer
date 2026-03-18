using WestcoastCars.Domain.Common;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Domain.Events.Vehicles
{
    public class VehicleCreatedEvent : DomainEvent
    {
        public Vehicle Vehicle { get; }

        public VehicleCreatedEvent(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }
    }
}
