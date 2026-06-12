
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class ManufacturerRepository(WestcoastCarsContext context) : Repository<Manufacturer>(context), IManufacturerRepository
{
    public async Task<Manufacturer?> FindByNameWithVehiclesAsync(string name)
    {
        var normalizedName = name.ToUpper();

        return await _context.Manufacturers
            .Include(m => m.Vehicles)
            .SingleOrDefaultAsync(m => m.Name.Equals(normalizedName, StringComparison.CurrentCultureIgnoreCase));
    }
}

