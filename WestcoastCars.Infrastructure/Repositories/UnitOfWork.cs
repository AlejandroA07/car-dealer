using MediatR;
using System.Collections;
using System.Linq;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WestcoastCarsContext _context;
    private readonly IMediator _mediator;
    private Hashtable _repositories;
    public IVehicleRepository VehicleRepository { get; }
    public IManufacturerRepository ManufacturerRepository { get; }
    public IFuelTypeRepository FuelTypeRepository { get; }
    public ITransmissionTypeRepository TransmissionTypeRepository { get; }
    public IServiceBookingRepository ServiceBookingRepository { get; }

    public UnitOfWork(WestcoastCarsContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
        _repositories = new Hashtable();
        VehicleRepository = new VehicleRepository(context);
        ManufacturerRepository = new ManufacturerRepository(context);
        FuelTypeRepository = new FuelTypeRepository(context);
        TransmissionTypeRepository = new TransmissionTypeRepository(context);
        ServiceBookingRepository = new ServiceBookingRepository(context);
    }

    public IRepository<T>? Repository<T>() where T : BaseEntity
    {
        var type = typeof(T).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(Repository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

            if (repositoryInstance != null) _repositories.Add(type, repositoryInstance);
        }

        return (IRepository<T>?)_repositories[type];
    }

    public async Task<int> CompleteAsync()
    {
        var result = await _context.SaveChangesAsync();

        await DispatchDomainEventsAsync();

        return result;
    }

    private async Task DispatchDomainEventsAsync()
    {
        var domainEvents = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(x => x.Entity.DomainEvents)
            .SelectMany(x => x)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }

        _context.ChangeTracker
            .Entries<BaseEntity>()
            .ToList()
            .ForEach(e => e.Entity.ClearDomainEvents());
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
