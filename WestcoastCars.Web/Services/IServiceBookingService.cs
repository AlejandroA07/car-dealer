using System.Collections.Generic;
using System.Threading.Tasks;
using WestcoastCars.Web.ViewModels.ServiceBooking;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Web.Services
{
    public interface IServiceBookingService
    {
        Task<bool> CreateBookingAsync(ServiceBookingViewModel model);
        Task<IEnumerable<ServiceBookingSummaryDto>> ListAllBookingsAsync();
    }
}
