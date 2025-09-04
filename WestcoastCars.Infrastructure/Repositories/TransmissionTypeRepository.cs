using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class TransmissionTypeRepository : ITransmissionTypeRepository
{
    private readonly WestcoastCarsContext _context;

    public TransmissionTypeRepository(WestcoastCarsContext context)
    {
        _context = context;
    }

    public async Task<TransmissionType?> FindByIdAsync(int id)
    {
        return await _context.TransmissionTypes.FindAsync(id);
    }

    public async Task<IReadOnlyList<TransmissionType>> ListAllAsync()
    {
        return await _context.TransmissionTypes.ToListAsync();
    }

    public async Task<IReadOnlyList<TransmissionType>> ListAsync(Expression<Func<TransmissionType, bool>> expression)
    {
        return await _context.TransmissionTypes.Where(expression).ToListAsync();
    }

    public async Task<TransmissionType?> FindByIdWithVehiclesAsync(int id)
    {
        return await _context.TransmissionTypes
            .Include(t => t.Vehicles)
            .ThenInclude(v => v.Manufacturer)
            .SingleOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<IReadOnlyList<TransmissionType>> FindByNameWithVehiclesAsync(string name)
    {
        return await _context.TransmissionTypes
            .Where(t => t.Name.ToUpper().StartsWith(name.ToUpper()))
            .Include(t => t.Vehicles)
            .ThenInclude(v => v.Manufacturer)
            .ToListAsync();
    }

    public void Add(TransmissionType transmissionType)
    {
        _context.TransmissionTypes.Add(transmissionType);
    }

    public void Delete(int id)
    {
        var transmissionType = _context.TransmissionTypes.Find(id);
        if (transmissionType is not null)
        {
            _context.TransmissionTypes.Remove(transmissionType);
        }
    }
}
