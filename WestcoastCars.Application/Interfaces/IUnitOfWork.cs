namespace WestcoastCars.Application.Interfaces;

/// <summary>
/// Implements the Unit of Work design pattern to manage transactions and aggregate repositories.
/// This ensures that all database operations within a single business transaction are handled
/// as a single, atomic unit. Repositories are exposed as properties, and changes are
/// committed to the database by calling the CompleteAsync method.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IVehicleRepository Vehicles { get; }
    IManufacturerRepository Manufacturers { get; }
    IFuelTypeRepository FuelTypes { get; }
    ITransmissionTypeRepository TransmissionTypes { get; }

    /// <summary>
    /// Saves all changes made across all repositories to the underlying database.
    /// </summary>
    /// <returns>The number of objects written to the underlying database.</returns>
    Task<int> CompleteAsync();
}
