
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
public class VehicleRepository : IVehicleRepository
{
    private readonly WestcoastCarsContext _context;

    public VehicleRepository(WestcoastCarsContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> FindByIdAsync(int id)
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionsType)
            .SingleOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vehicle?> FindByRegistrationNumberAsync(string regNo)
    {
        return await _context.Vehicles
            .SingleOrDefaultAsync(v => v.RegistrationNumber.ToUpper() == regNo.ToUpper());
    }

    public async Task<IReadOnlyList<Vehicle>> ListAllAsync()
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Vehicle>> ListAsync(Expression<Func<Vehicle, bool>> expression)
    {
        return await _context.Vehicles
            .Where(expression)
            .Include(v => v.Manufacturer)
            .ToListAsync();
    }

    public async Task AddAsync(Vehicle vehicle)
    {
        await _context.Vehicles.AddAsync(vehicle);
    }

    public Task UpdateAsync(Vehicle vehicle)
    {
        _context.Entry(vehicle).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle is not null)
        {
            _context.Vehicles.Remove(vehicle);
        }
    }
}
