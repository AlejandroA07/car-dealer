using WestcoastCars.Domain.Common;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Domain.Events.Vehicles
{
    public class VehicleSoldEvent : DomainEvent
    {
        public Vehicle Vehicle { get; }

        public VehicleSoldEvent(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }
    }
}
