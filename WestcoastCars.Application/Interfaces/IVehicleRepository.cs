
using System.Linq.Expressions;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

/// <summary>
/// Defines the contract for a repository that manages vehicle data.
/// This abstracts the data access logic from the application layer,
/// allowing for easier testing and maintenance.
/// </summary>
public interface IVehicleRepository
{
    /// <summary>
    /// Retrieves a vehicle by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the vehicle.</param>
    /// <returns>The vehicle entity if found; otherwise, null.</returns>
    Task<Vehicle?> FindByIdAsync(int id);

    /// <summary>
    /// Retrieves a vehicle by its registration number.
    /// </summary>
    /// <param name="regNo">The registration number of the vehicle.</param>
    /// <returns>The vehicle entity if found; otherwise, null.</returns>
    Task<Vehicle?> FindByRegistrationNumberAsync(string regNo);

    /// <summary>
    /// Retrieves a list of all vehicles.
    /// </summary>
    /// <returns>A read-only list of vehicle entities.</returns>
    Task<IReadOnlyList<Vehicle>> ListAllAsync();

    /// <summary>
    /// Retrieves a list of vehicles that match a specific condition.
    /// </summary>
    /// <param name="expression">The condition to filter vehicles by.</param>
    /// <returns>A read-only list of matching vehicle entities.</returns>
    Task<IReadOnlyList<Vehicle>> ListAsync(Expression<Func<Vehicle, bool>> expression);

    /// <summary>
    /// Adds a new vehicle to the repository.
    /// </summary>
    /// <param name="vehicle">The vehicle entity to add.</param>
    void Add(Vehicle vehicle);

    /// <summary>
    /// Updates an existing vehicle in the repository.
    /// </summary>
    /// <param name="vehicle">The vehicle entity to update.</param>
    void Update(Vehicle vehicle);

    /// <summary>
    /// Deletes a vehicle from the repository by its ID.
    /// </summary>
    /// <param name="id">The ID of the vehicle to delete.</param>
    void Delete(int id);
}
