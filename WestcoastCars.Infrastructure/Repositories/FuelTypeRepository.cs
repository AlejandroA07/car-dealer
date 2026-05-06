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
        var normalizedName = name.ToUpper();

        return await _context.FuelTypes
            .Include(f => f.Vehicles)
            .SingleOrDefaultAsync(f => f.Name.ToUpper() == normalizedName);
    }
}
