
namespace WestcoastCars.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IVehicleRepository VehicleRepository { get; }
    IManufacturerRepository ManufacturerRepository { get; }
    IFuelTypeRepository FuelTypeRepository { get; }
    ITransmissionTypeRepository TransmissionTypeRepository { get; }
    IServiceBookingRepository ServiceBookingRepository { get; }
    Task<int> CompleteAsync();
}
