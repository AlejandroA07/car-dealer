
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IVehicleRepository VehicleRepository { get; }
    IManufacturerRepository ManufacturerRepository { get; }
    IFuelTypeRepository FuelTypeRepository { get; }
    ITransmissionTypeRepository TransmissionTypeRepository { get; }
    IRepository<T>? Repository<T>() where T : BaseEntity;
    Task<int> CompleteAsync();
}

