using System.Collections.Generic;
using System.Threading.Tasks;
using westcoast_cars.web.ViewModels.ServiceBooking;
using WestcoastCars.Contracts.DTOs;

namespace westcoast_cars.web.Services
{
    public interface IServiceBookingService
    {
        Task<bool> CreateBookingAsync(ServiceBookingViewModel model);
        Task<IEnumerable<ServiceBookingSummaryDto>> ListAllBookingsAsync();
    }
}
