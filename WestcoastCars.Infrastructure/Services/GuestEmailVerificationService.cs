using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Services;
using WestcoastCars.Infrastructure.Authentication;

namespace WestcoastCars.Infrastructure.Services;

public class GuestEmailVerificationService : IGuestEmailVerificationService
{
    private const string EmailClaimType = "email";
    private const string CodeHashClaimType = "codeHash";
    private const string PurposeClaimType = "purpose";
    private const string SessionPurpose = "otp-session";
    private const string VerifiedPurpose = "guest-booking-verified";

    private readonly GuestVerificationSettings _settings;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<GuestEmailVerificationService> _logger;
    private readonly byte[] _secretBytes;

    public GuestEmailVerificationService(
        IOptions<GuestVerificationSettings> options,
        IEmailService emailService,
        IDateTimeProvider dateTimeProvider,
        ILogger<GuestEmailVerificationService> logger)
    {
        _settings = options.Value;
        _emailService = emailService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;

        if (_settings.Secret is null || _settings.Secret.Length < 32)
            throw new InvalidOperationException("GuestVerification:Secret must be at least 32 characters.");

        _secretBytes = Encoding.UTF8.GetBytes(_settings.Secret);
    }

    public async Task<string> RequestCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var code = GenerateCode();
        var codeHash = ComputeCodeHash(code);

        var sessionToken = CreateToken(
            purpose: SessionPurpose,
            email: normalizedEmail,
            expiry: TimeSpan.FromMinutes(_settings.CodeExpiryMinutes),
            extraClaims: [new Claim(CodeHashClaimType, codeHash)]);

        await _emailService.SendVerificationCodeAsync(email, code, _settings.CodeExpiryMinutes);

        return sessionToken;
    }

    public Task<string> ConfirmCodeAsync(string sessionToken, string code, CancellationToken cancellationToken = default)
    {
        var principal = ValidateToken(sessionToken, SessionPurpose);

        var email = principal.FindFirstValue(EmailClaimType)
            ?? throw new ValidationException("sessionToken", ["The verification session is invalid or has expired."]);
        var expectedHash = principal.FindFirstValue(CodeHashClaimType)
            ?? throw new ValidationException("sessionToken", ["The verification session is invalid or has expired."]);

        var actualHash = ComputeCodeHash(code);

        if (!FixedTimeEquals(expectedHash, actualHash))
        {
            _logger.LogWarning("Guest email verification code mismatch for a pending session.");
            throw new ValidationException("code", ["The verification code is incorrect."]);
        }

        var verifiedToken = CreateToken(
            purpose: VerifiedPurpose,
            email: email,
            expiry: TimeSpan.FromMinutes(_settings.VerifiedTokenExpiryMinutes));

        return Task.FromResult(verifiedToken);
    }

    public Task EnsureEmailIsVerifiedAsync(string? verifiedEmailToken, string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verifiedEmailToken))
        {
            throw new ValidationException("verifiedEmailToken", ["The email address must be verified before booking."]);
        }

        var principal = ValidateToken(verifiedEmailToken, VerifiedPurpose);

        var tokenEmail = principal.FindFirstValue(EmailClaimType)
            ?? throw new ValidationException("verifiedEmailToken", ["The email address must be verified before booking."]);

        if (!string.Equals(tokenEmail, NormalizeEmail(email), StringComparison.Ordinal))
        {
            throw new ValidationException("verifiedEmailToken", ["The verified token does not match the provided email address."]);
        }

        return Task.CompletedTask;
    }

    private string CreateToken(string purpose, string email, TimeSpan expiry, IEnumerable<Claim>? extraClaims = null)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(_secretBytes),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(EmailClaimType, email),
            new(PurposeClaimType, purpose),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (extraClaims is not null)
        {
            claims.AddRange(extraClaims);
        }

        var securityToken = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: _dateTimeProvider.UtcNow,
            expires: _dateTimeProvider.UtcNow.Add(expiry),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    private ClaimsPrincipal ValidateToken(string token, string expectedPurpose)
    {
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(_secretBytes),
            ClockSkew = TimeSpan.FromMinutes(1),
            LifetimeValidator = (notBefore, expires, _, parameters) =>
            {
                var now = _dateTimeProvider.UtcNow;
                return (!notBefore.HasValue || now >= notBefore.Value - parameters.ClockSkew)
                    && (!expires.HasValue || now < expires.Value + parameters.ClockSkew);
            }
        };

        ClaimsPrincipal principal;
        try
        {
            principal = handler.ValidateToken(token, validationParameters, out _);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            throw new ValidationException("token", ["The verification token is invalid or has expired."]);
        }

        var purpose = principal.FindFirstValue(PurposeClaimType);
        if (!string.Equals(purpose, expectedPurpose, StringComparison.Ordinal))
        {
            throw new ValidationException("token", ["The verification token is invalid or has expired."]);
        }

        return principal;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private string ComputeCodeHash(string code)
    {
        var hashBytes = HMACSHA256.HashData(_secretBytes, Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hashBytes);
    }

    private static bool FixedTimeEquals(string expectedHex, string actualHex)
    {
        if (expectedHex.Length != actualHex.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedHex),
            Encoding.UTF8.GetBytes(actualHex));
    }

    private string GenerateCode()
    {
        var max = (int)Math.Pow(10, _settings.CodeLength);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString(new string('0', _settings.CodeLength));
    }
}
