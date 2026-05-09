using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class ServiceBookingRepository : Repository<ServiceBooking>, IServiceBookingRepository
{
    private const int MaxPageSize = 100;

    public ServiceBookingRepository(WestcoastCarsContext context) : base(context)
    {
    }

    public async Task<PagedResult<ServiceBooking>> GetPagedAsync(PagedQueryDto pagination)
    {
        var page = Math.Max(1, pagination.Page);
        var pageSize = Math.Clamp(pagination.PageSize, 1, MaxPageSize);

        var query = _context.ServiceBookings.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ServiceBooking>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
