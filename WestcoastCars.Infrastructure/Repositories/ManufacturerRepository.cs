using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class ManufacturerRepository : IManufacturerRepository
{
    private readonly WestcoastCarsContext _context;

    public ManufacturerRepository(WestcoastCarsContext context)
    {
        _context = context;
    }

    public async Task<Manufacturer?> FindByIdAsync(int id)
    {
        return await _context.Manufacturers.FindAsync(id);
    }

    public async Task<IReadOnlyList<Manufacturer>> ListAllAsync()
    {
        return await _context.Manufacturers.ToListAsync();
    }

    public async Task<IReadOnlyList<Manufacturer>> ListAsync(Expression<Func<Manufacturer, bool>> expression)
    {
        return await _context.Manufacturers.Where(expression).ToListAsync();
    }

    public async Task<Manufacturer?> FindByNameWithVehiclesAsync(string name)
    {
        return await _context.Manufacturers
            .Include(m => m.Vehicles)
            .SingleOrDefaultAsync(m => m.Name.ToUpper() == name.ToUpper());
    }

    public void Add(Manufacturer manufacturer)
    {
        _context.Manufacturers.Add(manufacturer);
    }

    public void Delete(int id)
    {
        var manufacturer = _context.Manufacturers.Find(id);
        if (manufacturer is not null)
        {
            _context.Manufacturers.Remove(manufacturer);
        }
    }
}
