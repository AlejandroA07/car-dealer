using Microsoft.EntityFrameworkCore;
using WestcoastCars.Auth.Application.Common.Interfaces.Persistence;
using WestcoastCars.Auth.Domain.Entities;
using WestcoastCars.Auth.Infrastructure.Data;

namespace WestcoastCars.Auth.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
    }
}