using Microsoft.EntityFrameworkCore;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private readonly IVehicleTextSearchMatcher _textSearchMatcher;

    public VehicleRepository(WestcoastCarsContext context)
        : this(context, new CaseInsensitiveVehicleTextSearchMatcher())
    {
    }

    public VehicleRepository(WestcoastCarsContext context, IVehicleTextSearchMatcher textSearchMatcher) : base(context)
    {
        _textSearchMatcher = textSearchMatcher;
    }

    public async Task<Vehicle?> FindByRegistrationNumberAsync(string regNo)
    {
        var query = _context.Vehicles
            .AsNoTracking()
            .Where(v => v.RegistrationNumber != null)
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType);

        return await _textSearchMatcher.FindByRegistrationNumberAsync(query, regNo);
    }

    public async Task<PagedResult<Vehicle>> GetAllPagedAsync(PagedQueryDto pagination)
    {
        var query = _context.Vehicles.AsNoTracking().AsQueryable();
        return await ToPagedResultAsync(query, pagination.Page, pagination.PageSize);
    }

    public async Task<PagedResult<Vehicle>> GetUnsoldAsync(PagedQueryDto pagination)
    {
        var query = _context.Vehicles
            .AsNoTracking()
            .Where(v => !v.IsSold);

        return await ToPagedResultAsync(query, pagination.Page, pagination.PageSize);
    }

    public async Task<IEnumerable<Vehicle>> GetAllImportedFromBlocketAsync()
    {
        return await _context.Vehicles
            .Where(vehicle => vehicle.Source == "Blocket")
            .ToListAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetAllSourceRemovedFromBlocketAsync()
    {
        return await _context.Vehicles
            .Where(v => v.Source == "Blocket" && v.SourceStatus == "SourceRemoved")
            .ToListAsync();
    }

    public async Task<PagedResult<Vehicle>> SearchAsync(VehicleSearchDto search)
    {
        var query = _context.Vehicles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Make))
        {
            query = _textSearchMatcher.ApplyManufacturerFilter(query, search.Make);
        }

        if (!string.IsNullOrWhiteSpace(search.Model))
        {
            query = _textSearchMatcher.ApplyModelFilter(query, search.Model);
        }

        if (search.MinYear.HasValue)
        {
            query = query.Where(v => v.ModelYear >= search.MinYear.Value);
        }

        if (search.MaxYear.HasValue)
        {
            query = query.Where(v => v.ModelYear <= search.MaxYear.Value);
        }

        if (search.MinPrice.HasValue)
        {
            query = query.Where(v => v.Price >= search.MinPrice.Value);
        }

        if (search.MaxPrice.HasValue)
        {
            query = query.Where(v => v.Price <= search.MaxPrice.Value);
        }

        if (search.IsSold.HasValue)
        {
            query = query.Where(v => v.IsSold == search.IsSold.Value);
        }

        return await ToPagedResultAsync(query, search.Page, search.PageSize);
    }

    private static readonly (string Label, int Min, int? Max)[] MileageBands =
    [
        ("0 – 10 000 km", 0, 10000),
        ("10 000 – 20 000 km", 10000, 20000),
        ("20 000 – 30 000 km", 20000, 30000),
        ("30 000 – 40 000 km", 30000, 40000),
        ("40 000+ km", 40000, null),
    ];

    public async Task<IEnumerable<VehicleStatsByModelDto>> GetStatsByModelAsync()
    {
        var rows = await _context.Vehicles
            .Where(v => v.RegistrationNumber != null)
            .GroupBy(v => new { ManufacturerName = v.Manufacturer.Name, v.Model })
            .Select(g => new { FullName = g.Key.ManufacturerName + " " + g.Key.Model, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return rows.Select(x => new VehicleStatsByModelDto(x.FullName, x.Count));
    }

    public async Task<IEnumerable<VehicleStatsByMileageDto>> GetStatsByMileageAsync()
    {
        var result = new List<VehicleStatsByMileageDto>();
        foreach (var (label, min, max) in MileageBands)
        {
            var query = _context.Vehicles.Where(v => v.RegistrationNumber != null && v.Mileage >= min);
            if (max.HasValue) query = query.Where(v => v.Mileage < max.Value);
            var count = await query.CountAsync();
            result.Add(new VehicleStatsByMileageDto(label, min, max, count));
        }
        return result;
    }

    public async Task<VehicleStatsSummaryDto> GetStatsSummaryAsync()
    {
        var baseQuery = _context.Vehicles.Where(v => v.RegistrationNumber != null);
        var total = await baseQuery.CountAsync();
        var totalSold = await baseQuery.CountAsync(v => v.IsSold);
        var totalSourceRemoved = await baseQuery.CountAsync(v => v.SourceStatus == "SourceRemoved");
        return new VehicleStatsSummaryDto(total, totalSold, total - totalSold, totalSourceRemoved);
    }

    public async Task<IReadOnlyList<Vehicle>> GetForBulkDeleteAsync(string? model, bool? isSold, int? minMileage, int? maxMileage)
    {
        var query = _context.Vehicles.Where(v => v.RegistrationNumber != null).AsQueryable();
        if (model is not null) query = query.Where(v => v.Model == model);
        if (isSold.HasValue) query = query.Where(v => v.IsSold == isSold.Value);
        if (minMileage.HasValue) query = query.Where(v => v.Mileage >= minMileage.Value);
        if (maxMileage.HasValue) query = query.Where(v => v.Mileage < maxMileage.Value);
        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<Vehicle>> GetAllForDeleteAsync()
    {
        return await _context.Vehicles.ToListAsync();
    }

    public override async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.RegistrationNumber != null)
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .SingleOrDefaultAsync(v => v.Id == id);
    }

    public override async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.RegistrationNumber != null)
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .ToListAsync();
    }

    private async Task<PagedResult<Vehicle>> ToPagedResultAsync(IQueryable<Vehicle> query, int page, int pageSize)
    {
        query = query.Where(v => v.RegistrationNumber != null);

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1
            ? DefaultPageSize
            : Math.Min(pageSize, MaxPageSize);

        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)normalizedPageSize));
        var normalizedPageNumber = Math.Min(normalizedPage, totalPages);
        var items = await query
            .OrderBy(v => v.Id)
            .Include(v => v.Manufacturer)
            .Include(v => v.FuelType)
            .Include(v => v.TransmissionType)
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        return new PagedResult<Vehicle>
        {
            Items = items,
            TotalCount = totalCount,
            Page = normalizedPageNumber,
            PageSize = normalizedPageSize
        };
    }
}
