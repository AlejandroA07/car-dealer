using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Infrastructure.Data;

public class WestcoastCarsContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<FuelType> FuelTypes { get; set; }
    public DbSet<TransmissionType> TransmissionTypes { get; set; }
    public DbSet<ServiceBooking> ServiceBookings { get; set; }
    public WestcoastCarsContext(DbContextOptions<WestcoastCarsContext> options) : base(options) { }

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

        modelBuilder.Entity<Vehicle>()
            .Property(vehicle => vehicle.RegistrationNumber)
            .HasColumnType("citext");

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

        modelBuilder.Entity<ServiceBooking>()
            .HasOne(serviceBooking => serviceBooking.Vehicle)
            .WithMany(vehicle => vehicle.ServiceBookings)
            .HasForeignKey(serviceBooking => serviceBooking.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        // Manufacturer name uniqueness is migration-managed via a PostgreSQL lower("Name") index
        // because the same column also needs a trigram GIN index for fast ILIKE substring search.

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
