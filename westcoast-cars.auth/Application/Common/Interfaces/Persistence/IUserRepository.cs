using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Application.Common.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task AddUserAsync(User user);
}