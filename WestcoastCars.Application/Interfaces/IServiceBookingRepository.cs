using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IServiceBookingRepository : IRepository<ServiceBooking>
{
    Task<PagedResult<ServiceBooking>> GetPagedAsync(PagedQueryDto pagination);
}
