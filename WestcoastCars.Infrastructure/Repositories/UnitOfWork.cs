using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WestcoastCarsContext _context;

    public IVehicleRepository Vehicles { get; private set; }
    public IManufacturerRepository Manufacturers { get; private set; }
    public IFuelTypeRepository FuelTypes { get; private set; }
    public ITransmissionTypeRepository TransmissionTypes { get; private set; }

    public UnitOfWork(WestcoastCarsContext context)
    {
        _context = context;
        Vehicles = new VehicleRepository(_context);
        Manufacturers = new ManufacturerRepository(_context);
        FuelTypes = new FuelTypeRepository(_context);
        TransmissionTypes = new TransmissionTypeRepository(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
