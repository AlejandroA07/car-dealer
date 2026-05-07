using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WestcoastCarsContext _context;
    public IVehicleRepository VehicleRepository { get; }
    public IManufacturerRepository ManufacturerRepository { get; }
    public IFuelTypeRepository FuelTypeRepository { get; }
    public ITransmissionTypeRepository TransmissionTypeRepository { get; }
    public IServiceBookingRepository ServiceBookingRepository { get; }

    public UnitOfWork(WestcoastCarsContext context)
    {
        _context = context;
        VehicleRepository = new VehicleRepository(context);
        ManufacturerRepository = new ManufacturerRepository(context);
        FuelTypeRepository = new FuelTypeRepository(context);
        TransmissionTypeRepository = new TransmissionTypeRepository(context);
        ServiceBookingRepository = new ServiceBookingRepository(context);
    }

    public async Task<int> CompleteAsync()
    {
        try
        {
            var result = await _context.SaveChangesAsync();
            return result;
        }
        catch (DbUpdateException ex) when (TryMapUniqueConstraint(ex, out var conflictMessage))
        {
            throw new ConflictException(conflictMessage);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static bool TryMapUniqueConstraint(DbUpdateException exception, out string conflictMessage)
    {
        if (exception.InnerException is PostgresException postgresException &&
            postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            conflictMessage = postgresException.ConstraintName switch
            {
                "IX_Vehicles_RegistrationNumber" => "Vehicle with the same registration number already exists.",
                "IX_Manufacturers_Name" => "Manufacturer with the same name already exists.",
                "IX_FuelTypes_Name" => "FuelType with the same name already exists.",
                "IX_TransmissionTypes_Name" => "TransmissionType with the same name already exists.",
                _ => "A record with the same unique value already exists."
            };

            return true;
        }

        if (exception.InnerException is SqliteException sqliteException &&
            sqliteException.SqliteExtendedErrorCode == 2067)
        {
            conflictMessage = "A record with the same unique value already exists.";
            return true;
        }

        conflictMessage = string.Empty;
        return false;
    }
}
