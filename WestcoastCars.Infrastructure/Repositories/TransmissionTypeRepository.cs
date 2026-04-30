using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class TransmissionTypeRepository : Repository<TransmissionType>, ITransmissionTypeRepository
{
    public TransmissionTypeRepository(WestcoastCarsContext context) : base(context)
    {
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
        var normalizedName = name.ToUpper();

        return await _context.TransmissionTypes
            .Where(t => t.Name.ToUpper().StartsWith(normalizedName))
            .Include(t => t.Vehicles)
            .ThenInclude(v => v.Manufacturer)
            .ToListAsync();
    }
}