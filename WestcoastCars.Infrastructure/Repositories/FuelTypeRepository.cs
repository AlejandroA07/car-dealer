using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class FuelTypeRepository : IFuelTypeRepository
{
    private readonly WestcoastCarsContext _context;

    public FuelTypeRepository(WestcoastCarsContext context)
    {
        _context = context;
    }

    public async Task<FuelType?> FindByIdAsync(int id)
    {
        return await _context.FuelTypes.FindAsync(id);
    }

    public async Task<IReadOnlyList<FuelType>> ListAllAsync()
    {
        return await _context.FuelTypes.ToListAsync();
    }
    
    public async Task<IReadOnlyList<FuelType>> ListAsync(Expression<Func<FuelType, bool>> expression)
    {
        return await _context.FuelTypes.Where(expression).ToListAsync();
    }

    public async Task<FuelType?> FindByNameWithVehiclesAsync(string name)
    {
        return await _context.FuelTypes
            .Include(f => f.Vehicles)
            .SingleOrDefaultAsync(f => f.Name.ToUpper() == name.ToUpper());
    }

    public async Task AddAsync(FuelType fuelType)
    {
        await _context.FuelTypes.AddAsync(fuelType);
    }
}
