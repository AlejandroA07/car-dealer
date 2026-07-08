using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Services;
using WestcoastCars.Infrastructure.Authentication;
using WestcoastCars.Infrastructure.Services;
using Xunit;

namespace WestcoastCars.Infrastructure.Tests.Services;

public class GuestEmailVerificationServiceTests
{
    private const string Secret = "this-is-a-test-secret-that-is-at-least-32-chars-long";
    private const string OtherSecret = "a-completely-different-secret-that-is-also-32-chars";

    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    public GuestEmailVerificationServiceTests()
    {
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(() => _now);

        _emailServiceMock
            .Setup(s => s.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
    }

    private GuestEmailVerificationService CreateService(
        string secret = Secret,
        string issuer = "WestcoastCars.GuestVerification",
        string audience = "WestcoastCars.GuestVerification",
        int codeExpiryMinutes = 10,
        int verifiedTokenExpiryMinutes = 20)
    {
        var settings = new GuestVerificationSettings
        {
            Secret = secret,
            Issuer = issuer,
            Audience = audience,
            CodeLength = 6,
            CodeExpiryMinutes = codeExpiryMinutes,
            VerifiedTokenExpiryMinutes = verifiedTokenExpiryMinutes
        };

        return new GuestEmailVerificationService(
            Microsoft.Extensions.Options.Options.Create(settings),
            _emailServiceMock.Object,
            _dateTimeProviderMock.Object,
            NullLogger<GuestEmailVerificationService>.Instance);
    }

    private async Task<(string SessionToken, string Code)> RequestCodeAsync(GuestEmailVerificationService service, string email)
    {
        string? capturedCode = null;
        _emailServiceMock
            .Setup(s => s.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, code, _) => capturedCode = code)
            .Returns(Task.CompletedTask);

        var sessionToken = await service.RequestCodeAsync(email);
        return (sessionToken, capturedCode!);
    }

    [Fact]
    public async Task HappyPath_RoundTrip_Succeeds()
    {
        var service = CreateService();
        var (sessionToken, code) = await RequestCodeAsync(service, "guest@example.com");

        var verifiedToken = await service.ConfirmCodeAsync(sessionToken, code);

        await service.EnsureEmailIsVerifiedAsync(verifiedToken, "guest@example.com");
    }

    [Fact]
    public async Task EnsureEmailIsVerifiedAsync_IsCaseInsensitiveOnEmail()
    {
        var service = CreateService();
        var (sessionToken, code) = await RequestCodeAsync(service, "Guest@Example.com");

        var verifiedToken = await service.ConfirmCodeAsync(sessionToken, code);

        await service.EnsureEmailIsVerifiedAsync(verifiedToken, "guest@EXAMPLE.com");
    }

    [Fact]
    public async Task ConfirmCodeAsync_ThrowsValidationException_WhenCodeIsWrong()
    {
        var service = CreateService();
        var (sessionToken, _) = await RequestCodeAsync(service, "guest@example.com");

        await Assert.ThrowsAsync<ValidationException>(() => service.ConfirmCodeAsync(sessionToken, "000000"));
    }

    [Fact]
    public async Task ConfirmCodeAsync_ThrowsValidationException_WhenTokenWasSignedWithDifferentSecret()
    {
        var serviceA = CreateService(secret: Secret);
        var serviceB = CreateService(secret: OtherSecret);
        var (sessionToken, code) = await RequestCodeAsync(serviceA, "guest@example.com");

        await Assert.ThrowsAsync<ValidationException>(() => serviceB.ConfirmCodeAsync(sessionToken, code));
    }

    [Fact]
    public async Task ConfirmCodeAsync_ThrowsValidationException_WhenSessionTokenHasExpired()
    {
        var service = CreateService(codeExpiryMinutes: 10);
        var (sessionToken, code) = await RequestCodeAsync(service, "guest@example.com");

        _now = _now.AddMinutes(15);

        await Assert.ThrowsAsync<ValidationException>(() => service.ConfirmCodeAsync(sessionToken, code));
    }

    [Fact]
    public async Task EnsureEmailIsVerifiedAsync_ThrowsValidationException_WhenVerifiedTokenHasExpired()
    {
        var service = CreateService(verifiedTokenExpiryMinutes: 20);
        var (sessionToken, code) = await RequestCodeAsync(service, "guest@example.com");
        var verifiedToken = await service.ConfirmCodeAsync(sessionToken, code);

        _now = _now.AddMinutes(25);

        await Assert.ThrowsAsync<ValidationException>(() => service.EnsureEmailIsVerifiedAsync(verifiedToken, "guest@example.com"));
    }

    [Fact]
    public async Task EnsureEmailIsVerifiedAsync_ThrowsValidationException_WhenGivenASessionTokenInsteadOfAVerifiedToken()
    {
        var service = CreateService();
        var (sessionToken, _) = await RequestCodeAsync(service, "guest@example.com");

        await Assert.ThrowsAsync<ValidationException>(() => service.EnsureEmailIsVerifiedAsync(sessionToken, "guest@example.com"));
    }

    [Fact]
    public async Task ConfirmCodeAsync_ThrowsValidationException_WhenIssuerOrAudienceDiffers()
    {
        var serviceA = CreateService(issuer: "IssuerA", audience: "AudienceA");
        var serviceB = CreateService(issuer: "IssuerB", audience: "AudienceB");
        var (sessionToken, code) = await RequestCodeAsync(serviceA, "guest@example.com");

        await Assert.ThrowsAsync<ValidationException>(() => serviceB.ConfirmCodeAsync(sessionToken, code));
    }

    [Fact]
    public async Task EnsureEmailIsVerifiedAsync_ThrowsValidationException_WhenEmailDoesNotMatch()
    {
        var service = CreateService();
        var (sessionToken, code) = await RequestCodeAsync(service, "guest@example.com");
        var verifiedToken = await service.ConfirmCodeAsync(sessionToken, code);

        await Assert.ThrowsAsync<ValidationException>(() => service.EnsureEmailIsVerifiedAsync(verifiedToken, "someone-else@example.com"));
    }

    [Fact]
    public async Task EnsureEmailIsVerifiedAsync_ThrowsValidationException_WhenTokenIsNullOrEmpty()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() => service.EnsureEmailIsVerifiedAsync(null, "guest@example.com"));
        await Assert.ThrowsAsync<ValidationException>(() => service.EnsureEmailIsVerifiedAsync(string.Empty, "guest@example.com"));
    }

    [Fact]
    public void Constructor_Throws_WhenSecretIsTooShort()
    {
        var settings = new GuestVerificationSettings
        {
            Secret = "too-short",
            Issuer = "issuer",
            Audience = "audience"
        };

        Assert.Throws<InvalidOperationException>(() => new GuestEmailVerificationService(
            Microsoft.Extensions.Options.Options.Create(settings),
            _emailServiceMock.Object,
            _dateTimeProviderMock.Object,
            NullLogger<GuestEmailVerificationService>.Instance));
    }
}
