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

        modelBuilder.Entity<ServiceBooking>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()");

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.ExternalListingId);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(vehicle => vehicle.Source);
    }
}
