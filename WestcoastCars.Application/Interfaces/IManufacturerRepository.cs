using System.Linq.Expressions;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

/// <summary>
/// Defines the contract for a repository that manages manufacturer data.
/// </summary>
public interface IManufacturerRepository
{
    /// <summary>
    /// Retrieves a manufacturer by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the manufacturer.</param>
    /// <returns>The manufacturer entity if found; otherwise, null.</returns>
    Task<Manufacturer?> FindByIdAsync(int id);

    /// <summary>
    /// Retrieves a list of all manufacturers.
    /// </summary>
    /// <returns>A read-only list of manufacturer entities.</returns>
    Task<IReadOnlyList<Manufacturer>> ListAllAsync();
    
    /// <summary>
    /// Retrieves a list of manufacturers that match a specific condition.
    /// </summary>
    /// <param name="expression">The condition to filter manufacturers by.</param>
    /// <returns>A read-only list of matching manufacturer entities.</returns>
    Task<IReadOnlyList<Manufacturer>> ListAsync(Expression<Func<Manufacturer, bool>> expression);

    /// <summary>
    /// Finds a manufacturer by name, including its list of vehicles.
    /// </summary>
    /// <param name="name">The name of the manufacturer.</param>
    /// <returns>The manufacturer entity with its vehicles if found; otherwise, null.</returns>
    Task<Manufacturer?> FindByNameWithVehiclesAsync(string name);

    /// <summary>
    /// Adds a new manufacturer to the repository.
    /// </summary>
    /// <param name="manufacturer">The manufacturer entity to add.</param>
    void Add(Manufacturer manufacturer);

    /// <summary>
    /// Deletes a manufacturer from the repository.
    /// </summary>
    /// <param name="id">The ID of the manufacturer to delete.</param>
    void Delete(int id);
}
