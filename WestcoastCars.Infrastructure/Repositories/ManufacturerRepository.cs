
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class ManufacturerRepository : Repository<Manufacturer>, IManufacturerRepository
{
    public ManufacturerRepository(WestcoastCarsContext context) : base(context)
    {
    }

    public async Task<Manufacturer?> FindByNameWithVehiclesAsync(string name)
    {
        return await _context.Manufacturers
            .Include(m => m.Vehicles)
            .SingleOrDefaultAsync(m => m.Name.ToUpper() == name.ToUpper());
    }
}

