using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Infrastructure.Data;

public class WestcoastCarsContext(DbContextOptions<WestcoastCarsContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<FuelType> FuelTypes { get; set; }
    public DbSet<TransmissionType> TransmissionTypes { get; set; }
    public DbSet<ServiceBooking> ServiceBookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.HasPostgresExtension("pg_trgm");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(entityType => typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.CreatedAt))
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.UpdatedAt))
                .HasDefaultValueSql("NOW()");
        }

        modelBuilder.Entity<Manufacturer>()
            .Property(m => m.Name)
            .HasColumnType("citext");

        modelBuilder.Entity<Manufacturer>()
            .HasIndex(manufacturer => manufacturer.Name)
            .HasDatabaseName("IX_Manufacturers_Name_Trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        modelBuilder.Entity<FuelType>()
            .Property(fuelType => fuelType.Name)
            .HasColumnType("citext");

        modelBuilder.Entity<FuelType>()
            .HasIndex(fuelType => fuelType.Name)
            .HasDatabaseName("IX_FuelTypes_Name")
            .IsUnique();

        modelBuilder.Entity<TransmissionType>()
            .Property(transmissionType => transmissionType.Name)
            .HasColumnType("citext");

        modelBuilder.Entity<TransmissionType>()
            .HasIndex(transmissionType => transmissionType.Name)
            .HasDatabaseName("IX_TransmissionTypes_Name")
            .IsUnique();

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.RegistrationNumber)
            .HasColumnType("citext");

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.Model)
            .HasMaxLength(100);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.Description)
            .HasMaxLength(4000);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.ImageUrl)
            .HasMaxLength(500);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.ExternalListingId)
            .HasMaxLength(100);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.Source)
            .HasMaxLength(50);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.SourceUrl)
            .HasMaxLength(500);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.Color)
            .HasMaxLength(50);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.WheelDrive)
            .HasMaxLength(50);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.BodyType)
            .HasMaxLength(50);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.EngineVolume)
            .HasMaxLength(20);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.City)
            .HasMaxLength(100);

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.Address)
            .HasMaxLength(200);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.ExternalListingId);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.Source);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.IsSold);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.Model)
            .HasDatabaseName("IX_Vehicles_Model")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.RegistrationNumber)
            .HasDatabaseName("IX_Vehicles_RegistrationNumber")
            .IsUnique();

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.SourceStatus)
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.SourceStatus)
            .HasDatabaseName("IX_Vehicles_SourceStatus");

        modelBuilder.Entity<ServiceBooking>()
            .Property(sb => sb.VehicleRegistrationNumber)
            .HasMaxLength(10)
            .HasColumnType("citext");

        modelBuilder.Entity<ServiceBooking>()
            .HasIndex(sb => new { sb.BookingDate, sb.TimeSlot })
            .HasDatabaseName("IX_ServiceBookings_ActiveSlot")
            .IsUnique()
            .HasFilter($@"""Status"" NOT IN ({(int)BookingStatus.Cancelled}, {(int)BookingStatus.Completed})");

        modelBuilder.Entity<ServiceBooking>()
            .HasIndex(sb => sb.VehicleRegistrationNumber)
            .HasDatabaseName("IX_ServiceBookings_ActiveRegistrationNumber")
            .IsUnique()
            .HasFilter($@"""Status"" NOT IN ({(int)BookingStatus.Cancelled}, {(int)BookingStatus.Completed})");

        modelBuilder.Entity<ServiceBooking>()
            .Property(sb => sb.IdempotencyKey)
            .HasMaxLength(36);

        modelBuilder.Entity<ServiceBooking>()
            .HasIndex(sb => sb.IdempotencyKey)
            .HasDatabaseName("IX_ServiceBookings_IdempotencyKey")
            .IsUnique()
            .HasFilter(@"""IdempotencyKey"" IS NOT NULL");

        modelBuilder.Entity<ServiceBooking>()
            .Property(sb => sb.ServiceType)
            .HasMaxLength(50);

        modelBuilder.Entity<ServiceBooking>()
            .Property(sb => sb.CustomerName)
            .HasMaxLength(100);

        modelBuilder.Entity<ServiceBooking>()
            .Property(sb => sb.CustomerEmail)
            .HasMaxLength(256);

        modelBuilder.Entity<ServiceBooking>()
            .Property(sb => sb.CustomerPhone)
            .HasMaxLength(50);

        modelBuilder.Entity<ServiceBooking>()
            .Property(sb => sb.Description)
            .HasMaxLength(2000);

        modelBuilder.Entity<ServiceBooking>()
            .HasOne(serviceBooking => serviceBooking.Vehicle)
            .WithMany(vehicle => vehicle.ServiceBookings)
            .HasForeignKey(serviceBooking => serviceBooking.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

    }

    public override int SaveChanges()
    {
        SetAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
