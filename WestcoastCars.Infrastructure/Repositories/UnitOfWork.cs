using WestcoastCars.Application.Interfaces;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WestcoastCarsContext _context;
    public IVehicleRepository VehicleRepository { get; }
    public IManufacturerRepository ManufacturerRepository { get; }
    public IFuelTypeRepository FuelTypeRepository { get; }
    public ITransmissionTypeRepository TransmissionTypeRepository { get; }
    public IServiceBookingRepository ServiceBookingRepository { get; }

    public UnitOfWork(WestcoastCarsContext context)
    {
        _context = context;
        VehicleRepository = new VehicleRepository(context);
        ManufacturerRepository = new ManufacturerRepository(context);
        FuelTypeRepository = new FuelTypeRepository(context);
        TransmissionTypeRepository = new TransmissionTypeRepository(context);
        ServiceBookingRepository = new ServiceBookingRepository(context);
    }

    public async Task<int> CompleteAsync()
    {
        var result = await _context.SaveChangesAsync();
        return result;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
