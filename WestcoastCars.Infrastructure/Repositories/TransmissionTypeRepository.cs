using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class TransmissionTypeRepository(WestcoastCarsContext context) : Repository<TransmissionType>(context), ITransmissionTypeRepository
{
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
            .Where(t => t.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            .Include(t => t.Vehicles)
            .ThenInclude(v => v.Manufacturer)
            .ToListAsync();
    }
}
