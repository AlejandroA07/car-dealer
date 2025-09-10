using WestcoastCars.Auth.Application.Common.Interfaces.Authentication;
using WestcoastCars.Auth.Application.Common.Interfaces.Persistence;
using WestcoastCars.Auth.Application.Common.Interfaces.Services;
using WestcoastCars.Auth.Application.Services;
using WestcoastCars.Auth.Domain.Entities;

namespace WestcoastCars.Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;

    public AuthService(IJwtTokenGenerator jwtTokenGenerator, IPasswordHasher passwordHasher, IUserRepository userRepository)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
    }

    public async Task<AuthenticationResult> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user is null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            // In a real app, you might want to return a more specific error
            throw new Exception("Invalid credentials");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthenticationResult(user, token);
    }

    public async Task<AuthenticationResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        if (await _userRepository.GetUserByEmailAsync(email) is not null)
        {
            // In a real app, you might want to return a more specific error
            throw new Exception("User with this email already exists");
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password)
        };

        await _userRepository.AddUserAsync(user);

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthenticationResult(user, token);
    }
}