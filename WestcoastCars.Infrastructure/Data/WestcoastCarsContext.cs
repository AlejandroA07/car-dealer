using Microsoft.EntityFrameworkCore;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Infrastructure.Data
{
    public class WestcoastCarsContext : DbContext
    {
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<FuelType> FuelTypes { get; set; }
        public DbSet<TransmissionType> TransmissionTypes { get; set; }
        public DbSet<ServiceBooking> ServiceBookings { get; set; }
        public WestcoastCarsContext(DbContextOptions options) : base(options){}
    }
}