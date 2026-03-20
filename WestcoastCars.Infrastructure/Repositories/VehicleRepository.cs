using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(WestcoastCarsContext context) : base(context)
    {
    }

    public async Task<Vehicle?> FindByRegistrationNumberAsync(string regNo)
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .SingleOrDefaultAsync(v => v.RegistrationNumber.ToUpper() == regNo.ToUpper());
    }

    public async Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchDto search)
    {
        var query = _context.Vehicles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Make))
        {
            query = query.Where(v => v.Manufacturer.Name.Contains(search.Make));
        }

        if (!string.IsNullOrWhiteSpace(search.Model))
        {
            query = query.Where(v => v.Model.Contains(search.Model));
        }

        if (search.MinYear.HasValue)
        {
            var minYearStr = search.MinYear.Value.ToString();
            query = query.Where(v => v.ModelYear.CompareTo(minYearStr) >= 0);
        }

        if (search.MaxYear.HasValue)
        {
            var maxYearStr = search.MaxYear.Value.ToString();
            query = query.Where(v => v.ModelYear.CompareTo(maxYearStr) <= 0);
        }

        if (search.MinPrice.HasValue)
        {
            query = query.Where(v => v.Value >= search.MinPrice.Value);
        }

        if (search.MaxPrice.HasValue)
        {
            query = query.Where(v => v.Value <= search.MaxPrice.Value);
        }

        if (search.IsSold.HasValue)
        {
            query = query.Where(v => v.IsSold == search.IsSold.Value);
        }

        return await query
            .Include(v => v.Manufacturer)

            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .ToListAsync();
    }

    public new async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .SingleOrDefaultAsync(v => v.Id == id);
    }

    public new async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .ToListAsync();
    }
}