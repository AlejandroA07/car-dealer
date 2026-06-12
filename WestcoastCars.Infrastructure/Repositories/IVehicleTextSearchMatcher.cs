using Microsoft.EntityFrameworkCore;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Infrastructure.Repositories;

public interface IVehicleTextSearchMatcher
{
    Task<Vehicle?> FindByRegistrationNumberAsync(IQueryable<Vehicle> query, string registrationNumber);
    IQueryable<Vehicle> ApplyManufacturerFilter(IQueryable<Vehicle> query, string make);
    IQueryable<Vehicle> ApplyModelFilter(IQueryable<Vehicle> query, string model);
}

public class CaseInsensitiveVehicleTextSearchMatcher : IVehicleTextSearchMatcher
{
    public Task<Vehicle?> FindByRegistrationNumberAsync(IQueryable<Vehicle> query, string registrationNumber)
    {
        var normalizedRegistrationNumber = registrationNumber.ToLowerInvariant();
        return query.SingleOrDefaultAsync(vehicle => vehicle.RegistrationNumber!.Equals(normalizedRegistrationNumber, StringComparison.CurrentCultureIgnoreCase));
    }

    public IQueryable<Vehicle> ApplyManufacturerFilter(IQueryable<Vehicle> query, string make)
    {
        return query.Where(vehicle => vehicle.Manufacturer.Name.Contains(make, StringComparison.OrdinalIgnoreCase));
    }

    public IQueryable<Vehicle> ApplyModelFilter(IQueryable<Vehicle> query, string model)
    {
        return query.Where(vehicle => vehicle.Model.Contains(model, StringComparison.OrdinalIgnoreCase));
    }
}

public class PostgreSqlVehicleTextSearchMatcher : IVehicleTextSearchMatcher
{
    public Task<Vehicle?> FindByRegistrationNumberAsync(IQueryable<Vehicle> query, string registrationNumber)
    {
        return query.SingleOrDefaultAsync(vehicle => vehicle.RegistrationNumber == registrationNumber);
    }

    public IQueryable<Vehicle> ApplyManufacturerFilter(IQueryable<Vehicle> query, string make)
    {
        var pattern = $"%{EscapeLikePattern(make)}%";
        return query.Where(vehicle => EF.Functions.ILike(vehicle.Manufacturer.Name, pattern, "\\"));
    }

    public IQueryable<Vehicle> ApplyModelFilter(IQueryable<Vehicle> query, string model)
    {
        var pattern = $"%{EscapeLikePattern(model)}%";
        return query.Where(vehicle => EF.Functions.ILike(vehicle.Model, pattern, "\\"));
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
}
