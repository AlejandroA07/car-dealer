
namespace WestcoastCars.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IVehicleRepository VehicleRepository { get; }
    IManufacturerRepository ManufacturerRepository { get; }
    IFuelTypeRepository FuelTypeRepository { get; }
    ITransmissionTypeRepository TransmissionTypeRepository { get; }
    IServiceBookingRepository ServiceBookingRepository { get; }
    // Prefer CompleteOrThrowAsync (extension) in command handlers — converts DbUpdateException to
    // PersistenceException so the global error handler returns a meaningful response instead of a 500.
    // Use CompleteAsync only when the caller must handle DbUpdateException directly.
    Task<int> CompleteAsync();
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
