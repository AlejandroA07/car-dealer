using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WestcoastCars.Auth.Infrastructure.Data;
using WestcoastCars.Auth.Application.Common.Interfaces.Persistence;
using WestcoastCars.Auth.Infrastructure.Persistence;
using WestcoastCars.Auth.Application.Common.Interfaces.Authentication;
using WestcoastCars.Auth.Application.Common.Interfaces.Services;
using WestcoastCars.Auth.Infrastructure.Authentication;
using WestcoastCars.Auth.Infrastructure.Services;
using WestcoastCars.Auth.Application.Services;

namespace WestcoastCars.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
            ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }
}