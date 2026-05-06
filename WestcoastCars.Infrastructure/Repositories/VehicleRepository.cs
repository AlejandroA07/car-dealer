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
        var normalizedRegNo = regNo.ToUpper();

        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .SingleOrDefaultAsync(v => v.RegistrationNumber.ToUpper() == normalizedRegNo);
    }

    public async Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchDto search)
    {
        var query = _context.Vehicles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Make))
        {
            var normalizedMake = search.Make.ToUpper();
            query = query.Where(v => v.Manufacturer.Name.ToUpper().Contains(normalizedMake));
        }

        if (!string.IsNullOrWhiteSpace(search.Model))
        {
            var normalizedModel = search.Model.ToUpper();
            query = query.Where(v => v.Model.ToUpper().Contains(normalizedModel));
        }

        if (search.MinYear.HasValue)
        {
            query = query.Where(v => Convert.ToInt32(v.ModelYear) >= search.MinYear.Value);
        }

        if (search.MaxYear.HasValue)
        {
            query = query.Where(v => Convert.ToInt32(v.ModelYear) <= search.MaxYear.Value);
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

    public override async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .SingleOrDefaultAsync(v => v.Id == id);
    }

    public override async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .ToListAsync();
    }
}
