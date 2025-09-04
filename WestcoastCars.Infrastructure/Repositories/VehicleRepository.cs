
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
            .Include(v => v.TransmissionType)
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

    public void Add(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
    }

    public void Update(Vehicle vehicle)
    {
        _context.Entry(vehicle).State = EntityState.Modified;
    }

    public void Delete(int id)
    {
        var vehicle = _context.Vehicles.Find(id);
        if (vehicle is not null)
        {
            _context.Vehicles.Remove(vehicle);
        }
    }
}
