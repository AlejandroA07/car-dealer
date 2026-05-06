using WestcoastCars.Application.Exceptions;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public static class RepositoryExtensions
{
    public static async Task ThrowIfNameExistsAsync<T>(
        this IRepository<T> repository,
        string name,
        string entityName,
        int? excludingId = null) where T : NamedEntity
    {
        var normalizedName = name.ToUpper();
        var existing = await repository.FirstOrDefaultAsync(entity => entity.Name.ToUpper() == normalizedName);
        if (existing != null && (!excludingId.HasValue || existing.Id != excludingId.Value))
        {
            throw new ConflictException($"{entityName} with name '{name}' already exists.");
        }
    }
}
