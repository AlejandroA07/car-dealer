using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class FuelTypeRepository(WestcoastCarsContext context) : Repository<FuelType>(context), IFuelTypeRepository
{
    public async Task<FuelType?> FindByNameWithVehiclesAsync(string name)
    {
        var normalizedName = name.ToUpper();

        return await _context.FuelTypes
            .Include(f => f.Vehicles)
            .SingleOrDefaultAsync(f => f.Name.Equals(normalizedName, StringComparison.CurrentCultureIgnoreCase));
    }
}
