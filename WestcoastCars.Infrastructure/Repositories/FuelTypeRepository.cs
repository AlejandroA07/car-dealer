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

    
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class FuelTypeRepository : Repository<FuelType>, IFuelTypeRepository
{
    public FuelTypeRepository(WestcoastCarsContext context) : base(context)
    {
    }

    public async Task<FuelType?> FindByNameWithVehiclesAsync(string name)
    {
        return await _context.FuelTypes
            .Include(f => f.Vehicles)
            .SingleOrDefaultAsync(f => f.Name.ToUpper() == name.ToUpper());
    }
}


    public void Add(FuelType fuelType)
    {
        _context.FuelTypes.Add(fuelType);
    }

    public void Delete(int id)
    {
        var fuelType = _context.FuelTypes.Find(id);
        if (fuelType is not null)
        {
            _context.FuelTypes.Remove(fuelType);
        }
    }
}
