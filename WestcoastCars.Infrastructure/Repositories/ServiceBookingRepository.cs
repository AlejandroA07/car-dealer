using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories
{
    public class ServiceBookingRepository : Repository<ServiceBooking>, IServiceBookingRepository
    {
        public ServiceBookingRepository(WestcoastCarsContext context) : base(context)
        {
        }
    }
}
