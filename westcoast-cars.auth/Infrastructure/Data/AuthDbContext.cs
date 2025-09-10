using Microsoft.EntityFrameworkCore;
using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
}