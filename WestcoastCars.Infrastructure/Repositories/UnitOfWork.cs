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

    public UnitOfWork(WestcoastCarsContext context, IVehicleTextSearchMatcher vehicleTextSearchMatcher)
    {
        _context = context;
        VehicleRepository = new VehicleRepository(context, vehicleTextSearchMatcher);
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
        catch (DbUpdateException ex) when (TryMapForeignKeyConstraint(ex, out var relationException))
        {
            throw relationException;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var executionStrategy = _context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await action();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
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
                "IX_ServiceBookings_ActiveSlot" => "Det valda tidsfönstret är redan bokat. Välj ett annat.",
                "IX_ServiceBookings_ActiveRegistrationNumber" => "Det finns redan en aktiv bokning för detta registreringsnummer.",
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

    private static bool TryMapForeignKeyConstraint(DbUpdateException exception, out Exception mappedException)
    {
        if (exception.InnerException is PostgresException postgresException &&
            postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            mappedException = postgresException.ConstraintName switch
            {
                "FK_Vehicles_Manufacturers_ManufacturerId" => new NotFoundException("Manufacturer no longer exists."),
                "FK_Vehicles_FuelTypes_FuelTypeId" => new NotFoundException("Fuel type no longer exists."),
                "FK_Vehicles_TransmissionTypes_TransmissionTypeId" => new NotFoundException("Transmission type no longer exists."),
                _ => new ConflictException("The operation conflicts with existing related data.")
            };

            return true;
        }

        if (exception.InnerException is SqliteException sqliteException &&
            sqliteException.SqliteErrorCode == 19 &&
            sqliteException.Message.Contains("FOREIGN KEY constraint failed", StringComparison.OrdinalIgnoreCase))
        {
            mappedException = new ConflictException("The operation conflicts with existing related data.");
            return true;
        }

        mappedException = new ConflictException("The operation conflicts with existing related data.");
        return false;
    }
}
