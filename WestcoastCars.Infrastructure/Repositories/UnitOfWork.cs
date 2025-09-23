
using System.Collections;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WestcoastCarsContext _context;
    private Hashtable _repositories;
    public IVehicleRepository VehicleRepository { get; }
    public IManufacturerRepository ManufacturerRepository { get; }
    public IFuelTypeRepository FuelTypeRepository { get; }
    public ITransmissionTypeRepository TransmissionTypeRepository { get; }

    public UnitOfWork(WestcoastCarsContext context)
    {
        _context = context;
        VehicleRepository = new VehicleRepository(context);
        ManufacturerRepository = new ManufacturerRepository(context);
        FuelTypeRepository = new FuelTypeRepository(context);
        TransmissionTypeRepository = new TransmissionTypeRepository(context);
    }

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        if (_repositories == null) _repositories = new Hashtable();

        var type = typeof(T).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(Repository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

            _repositories.Add(type, repositoryInstance);
        }

        return (IRepository<T>)_repositories[type];
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

