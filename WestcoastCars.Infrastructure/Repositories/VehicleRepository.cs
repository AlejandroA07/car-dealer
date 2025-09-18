
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

/// <summary>
/// Implements the IVehicleRepository interface to provide concrete data access
/// logic for vehicle entities using Entity Framework Core.
/// </summary>

using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
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
            .SingleOrDefaultAsync(v => v.RegistrationNumber.ToUpper() == regNo.ToUpper());
    }

    public override async Task<Vehicle> GetByIdAsync(int id)
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
            .ToListAsync();
    }
}

