using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WestcoastCars.Application.Common.Interfaces.Authentication;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Models.Authentication;

namespace WestcoastCars.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtTokenGenerator(
        IOptions<JwtSettings> jwtOptions,
        IDateTimeProvider dateTimeProvider)
    {
        _jwtSettings = jwtOptions.Value;
        _dateTimeProvider = dateTimeProvider;

        if (_jwtSettings.Secret is null || _jwtSettings.Secret.Length < 32)
            throw new InvalidOperationException("JwtSettings:Secret must be at least 32 characters.");
    }

    public Task<string> GenerateTokenAsync(AuthenticatedUser user, IEnumerable<string> roles)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var securityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            claims: claims,
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
        return Task.FromResult(token);
    }
}
